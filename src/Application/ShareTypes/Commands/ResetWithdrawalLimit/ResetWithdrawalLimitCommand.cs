using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace JahnDigital.StudentBank.Application.ShareTypes.Commands.ResetWithdrawalLimit;

public record ResetWithdrawalLimitCommand(long ShareTypeId) : IRequest;

public class ResetWithdrawalLimitCommandHandler : IRequestHandler<ResetWithdrawalLimitCommand>
{
    private readonly IAppDbContext _context;
    public ResetWithdrawalLimitCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task Handle(ResetWithdrawalLimitCommand request, CancellationToken cancellationToken)
    {
        var shareType = await _context.ShareTypes
                .Where(x => x.Id == request.ShareTypeId)
                .SingleOrDefaultAsync(cancellationToken: cancellationToken)
            ?? throw new ShareTypeNotFoundException(request.ShareTypeId);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var query = _context.Shares.Where(x => x.ShareTypeId == request.ShareTypeId);
        int count = await query.CountAsync(cancellationToken: cancellationToken);

        for (int i = 0; i < count; i += 100)
        {
            var shares = await query.Skip(i).Take(100).ToListAsync(cancellationToken: cancellationToken);

            foreach (var share in shares)
            {
                share.LimitedWithdrawalCount = 0;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        shareType.WithdrawalLimitLastReset = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
