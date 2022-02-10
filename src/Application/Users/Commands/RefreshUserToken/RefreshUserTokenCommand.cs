using JahnDigital.StudentBank.Application.Common.DTOs;
using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Users.Commands.RefreshUserToken;

public record RefreshUserTokenCommand(string Token, string IpAddress) : IRequest<AuthenticateResponse>;

public class RefreshUserTokenCommandHandler : IRequestHandler<RefreshUserTokenCommand, AuthenticateResponse>
{
    private readonly IAppDbContext _context;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly string _secret;
    private readonly int _tokenLifetime;

    public RefreshUserTokenCommandHandler(IAppDbContext context, IJwtTokenGenerator tokenGenerator, string secret, int tokenLifetime)
    {
        _context = context;
        _tokenGenerator = tokenGenerator;
        _secret = secret;
        _tokenLifetime = tokenLifetime;
    }
    
    public async Task<AuthenticateResponse> Handle(RefreshUserTokenCommand request, CancellationToken cancellationToken)
    {
        User user = await _context.Users
                .Include(x => x.Role)
                .SingleOrDefaultAsync(
                    x => x.RefreshTokens.Any(t => t.Token == request.Token)
                        && x.DateDeleted == null,
                cancellationToken: cancellationToken)
            ?? throw new NotFoundException("Invalid refresh token.");

        user.DateLastLogin = DateTime.UtcNow;

        var newToken = _tokenGenerator.GenerateRefreshToken(request.IpAddress);
        RefreshToken refreshToken = user.RefreshTokens.Single(x => x.Token == request.Token);
        refreshToken.Revoked = DateTime.UtcNow;
        refreshToken.RevokedByIpAddress = request.IpAddress;
        refreshToken.ReplacedByToken = newToken.Token;

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

        string jwtToken = _tokenGenerator.Generate(tokenRequest);
        await _context.SaveChangesAsync(cancellationToken);
        return new AuthenticateResponse(user, jwtToken, newToken.Token);
    }
}
