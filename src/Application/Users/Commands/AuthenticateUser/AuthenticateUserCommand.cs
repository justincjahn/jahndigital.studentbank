using JahnDigital.StudentBank.Application.Common.DTOs;
using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Users.Commands.AuthenticateUser;

public record AuthenticateUserCommand(string Username, string Password, string IpAddress) : IRequest<AuthenticateResponse>;

public class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, AuthenticateResponse>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly string _secret;
    private readonly int _tokenLifetime;

    public AuthenticateUserCommandHandler(IAppDbContext context, IPasswordHasher hasher, IJwtTokenGenerator tokenGenerator, string secret, int tokenLifetime)
    {
        _context = context;
        _hasher = hasher;
        _tokenGenerator = tokenGenerator;
        _secret = secret;
        _tokenLifetime = tokenLifetime;
    }
    
    public async Task<AuthenticateResponse> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        User user = await _context.Users
                .Include(x => x.Role)
                .SingleOrDefaultAsync(
                    x => x.Email == request.Username.ToLower()
                        && x.DateDeleted == null
                        && x.DateRegistered == null, cancellationToken: cancellationToken)
            ?? throw new NotFoundException($"Username or password is not correct.");

        var valid = await _hasher.ValidateAsync(user.Password, request.Password);
        if (!valid) throw new NotFoundException("Username or password is not correct.");

        user.DateLastLogin = DateTime.UtcNow;

        var tokenRequest = new JwtTokenRequest()
        {
            JwtSecret = _secret,
            Type = UserType.User,
            Id = user.Id,
            Username = user.Email,
            Role = user.Role.Name,
            Email = user.Email,
            Expires = _tokenLifetime
        };

        string? jwtToken = _tokenGenerator.Generate(tokenRequest);
        RefreshToken refreshToken = _tokenGenerator.GenerateRefreshToken(request.IpAddress);
        user.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);
        return new AuthenticateResponse(user, jwtToken, refreshToken.Token);
    }
}
