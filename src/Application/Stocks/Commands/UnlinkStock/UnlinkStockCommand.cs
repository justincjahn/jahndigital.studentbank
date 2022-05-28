using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Stocks.Commands.UnlinkStock;

public record UnlinkStockCommand(long StockId, long InstanceId) : IRequest;

public class UnlinkStockCommandHandler : IRequestHandler<UnlinkStockCommand>
{
    private readonly IAppDbContext _context;

    public UnlinkStockCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task<Unit> Handle(UnlinkStockCommand request, CancellationToken cancellationToken)
    {
        var hasInstance = await _context.Instances.AnyAsync(x => x.Id == request.InstanceId, cancellationToken);

        if (!hasInstance)
        {
            throw new NotFoundException(nameof(Instance), request.InstanceId);
        }

        var hasStock = await _context.Stocks.AnyAsync(x => x.Id == request.StockId, cancellationToken);

        if (!hasStock)
        {
            throw new NotFoundException(nameof(Stock), request.StockId);
        }

        var link = await _context.StockInstances
            .Where(x => x.StockId == request.StockId && x.InstanceId == request.InstanceId)
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException(nameof(StockInstance), new object[] { request.StockId, request.InstanceId });

        var hasIssuedShares = await _context.StudentStocks
            .Include(x => x.Student)
            .ThenInclude(x => x.Group)
            .Where(x =>
                x.Student.Group.InstanceId == request.InstanceId
                && x.StockId == request.StockId
                && x.SharesOwned > 0)
            .AnyAsync(cancellationToken);

        if (hasIssuedShares)
        {
            throw new InvalidOperationException(
                "There are still students in this instance who own shares of this stock.  Please buy them out first!");
        }

        _context.StockInstances.Remove(link);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
