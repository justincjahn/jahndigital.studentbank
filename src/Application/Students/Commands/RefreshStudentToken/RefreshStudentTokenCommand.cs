using JahnDigital.StudentBank.Application.Common.DTOs;
using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Role = JahnDigital.StudentBank.Domain.Enums.Role;

namespace JahnDigital.StudentBank.Application.Students.Commands.RefreshStudentToken;

public record RefreshStudentTokenCommand(string Token, string IpAddress) : IRequest<AuthenticateResponse>;

public class RefreshStudentTokenCommandHandler : IRequestHandler<RefreshStudentTokenCommand, AuthenticateResponse>
{
    private readonly IAppDbContext _context;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public RefreshStudentTokenCommandHandler(IAppDbContext context, IJwtTokenGenerator tokenGenerator)
    {
        _context = context;
        _tokenGenerator = tokenGenerator;
    }
    
    public async Task<AuthenticateResponse> Handle(RefreshStudentTokenCommand request, CancellationToken cancellationToken)
    {
        var student = await _context.Students.SingleOrDefaultAsync(x =>
                x.DateDeleted == null
                && x.DateRegistered != null
                && x.RefreshTokens.Any(t => t.Token == request.Token
            ),
            cancellationToken: cancellationToken)
            ?? throw new NotFoundException("Invalid refresh token.");

        var refreshToken = student.RefreshTokens.Single(x => x.Token == request.Token);

        var newToken = _tokenGenerator.GenerateRefreshToken(request.IpAddress);
        refreshToken.Revoked = DateTime.UtcNow;
        refreshToken.RevokedByIpAddress = request.IpAddress;
        refreshToken.ReplacedByToken = newToken.Token;
        student.RefreshTokens.Add(newToken);
        await _context.SaveChangesAsync(cancellationToken);

        var tokenRequest = new JwtTokenRequest()
        {
            Type = UserType.Student,
            Id = student.Id,
            Username = student.AccountNumber,
            Role = Role.Student.Name,
            Email = student.Email ?? "",
            FirstName = student.FirstName,
            LastName = student.LastName
        };

        var jwtToken = _tokenGenerator.Generate(tokenRequest);
        return new AuthenticateResponse(student, jwtToken, newToken.Token);
    }
}
