using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Application.Students.Commands.RefreshStudentToken;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Stocks.Commands.DeleteStock;

public record DeleteStockCommand(long StockId) : IRequest;

public class DeleteStockCommandHandler : IRequestHandler<DeleteStockCommand>
{
    private readonly IAppDbContext _context;

    public DeleteStockCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteStockCommand request, CancellationToken cancellationToken)
    {
        var stock = await _context.Stocks.FindAsync(new object?[] { request.StockId }, cancellationToken)
            ?? throw new NotFoundException(nameof(Stock), request.StockId);

        var hasPurchases = await _context.StudentStocks.AnyAsync(x => x.SharesOwned > 0 && x.StockId == stock.Id, cancellationToken);

        if (hasPurchases)
        {
            throw new InvalidOperationException(
                "There are still students who own shares of this stock.  Please buy them out first.");
        }

        bool hasLinks = await _context.StockInstances.AnyAsync(x => x.StockId == request.StockId, cancellationToken);

        if (hasLinks)
        {
            throw new InvalidOperationException("Cannot delete a stock that's still linked to an instance.");
        }

        stock.DateDeleted = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
