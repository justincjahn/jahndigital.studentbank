using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Stocks.Commands.PurgeStockHistory;

public record PurgeStockHistoryCommand(long StockId, DateTime PurgeUptoDate) : IRequest<IEnumerable<StockHistory>>;

public class PurgeStockHistoryCommandHandler : IRequestHandler<PurgeStockHistoryCommand, IEnumerable<StockHistory>>
{
    private readonly IAppDbContext _context;

    public PurgeStockHistoryCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task<IEnumerable<StockHistory>> Handle(PurgeStockHistoryCommand request, CancellationToken cancellationToken)
    {
        var stock = await _context.Stocks.FindAsync(new object?[] { request.StockId }, cancellationToken)
            ?? throw new NotFoundException(nameof(Stock), request.StockId);

        var history = await _context.StockHistory
            .Where(x => x.StockId == request.StockId && x.DateChanged < request.PurgeUptoDate)
            .ToListAsync(cancellationToken);

        _context.StockHistory.RemoveRange(history);

        await _context.SaveChangesAsync(cancellationToken);

        return history;
    }
}
