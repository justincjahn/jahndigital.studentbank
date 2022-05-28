using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Stocks.Commands.RestoreStock;

public record RestoreStockCommand(long StockId) : IRequest;

public class RestoreStockCommandHandler : IRequestHandler<RestoreStockCommand>
{
    private readonly IAppDbContext _context;

    public RestoreStockCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task<Unit> Handle(RestoreStockCommand request, CancellationToken cancellationToken)
    {
        var stock = await _context.Stocks.FindAsync(new object?[] { request.StockId }, cancellationToken)
            ?? throw new NotFoundException(nameof(Stock), request.StockId);

        stock.DateDeleted = null;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
