using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Users.Commands.RevokeUserToken;

public record RevokeUserTokenCommand(string Token, string IpAddress) : IRequest;

public class RevokeUserTokenCommandHandler: IRequestHandler<RevokeUserTokenCommand>
{
    private readonly IAppDbContext _context;

    public RevokeUserTokenCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RevokeUserTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.SingleOrDefaultAsync(x => x.RefreshTokens.Any(t => t.Token == request.Token), cancellationToken: cancellationToken)
            ?? throw new NotFoundException("Invalid refresh token.");

        var refreshToken = user.RefreshTokens.Single(x => x.Token == request.Token);
        if (!refreshToken.IsActive) return;

        refreshToken.Revoked = DateTime.UtcNow;
        refreshToken.RevokedByIpAddress = request.IpAddress;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
