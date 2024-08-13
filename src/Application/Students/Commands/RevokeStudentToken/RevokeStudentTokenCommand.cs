using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Students.Commands.RevokeStudentToken;

public record RevokeStudentTokenCommand(string Token, string IpAddress) : IRequest;

public class RevokeStudentTokenCommandHandler : IRequestHandler<RevokeStudentTokenCommand>
{
    private readonly IAppDbContext _context;

    public RevokeStudentTokenCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task Handle(RevokeStudentTokenCommand request, CancellationToken cancellationToken)
    {
        var student = await _context.Students.SingleOrDefaultAsync(
                x => x.RefreshTokens.Any(t => t.Token == request.Token),
                cancellationToken: cancellationToken
            )
        ?? throw new NotFoundException("Invalid refresh token.");

        var refreshToken = student.RefreshTokens.Single(x => x.Token == request.Token);
        if (!refreshToken.IsActive) return;

        refreshToken.Revoked = DateTime.UtcNow;
        refreshToken.RevokedByIpAddress = request.IpAddress;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
