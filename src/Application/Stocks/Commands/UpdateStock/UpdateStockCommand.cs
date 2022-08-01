using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Stocks.Commands.UpdateStock;

public record UpdateStockCommand : IRequest
{
    public long StockId { get; init; }
    public string? Name { get; init; }
    public string? Symbol { get; init; }
    public long? TotalShares { get; init; }
    public Money? CurrentValue { get; init; }
    public string? RawDescription { get; init; }
}

public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand>
{
    private readonly IAppDbContext _context;
    private readonly ITextFormatter _formatter;

    public UpdateStockCommandHandler(IAppDbContext context, ITextFormatter formatter)
    {
        _context = context;
        _formatter = formatter;
    }

    public async Task<Unit> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        var stock = await _context.Stocks.FindAsync(new object?[] { request.StockId }, cancellationToken)
            ?? throw new NotFoundException(nameof(Stock), request.StockId);

        if (request.Name is not null && request.Name != stock.Name)
        {
            bool stockExists = await _context.Stocks.Where(x => x.Name == request.Name && x.Id != request.StockId)
                .AnyAsync(cancellationToken);

            if (stockExists)
            {
                throw new InvalidOperationException($"A stock named {request.Name} already exists.");
            }

            stock.Name = request.Name;
        }

        if (request.Symbol is not null && request.Symbol != stock.Symbol)
        {
            bool stockExists = await _context.Stocks.Where(x => x.Symbol == request.Symbol && x.Id != request.StockId)
                .AnyAsync(cancellationToken);

            if (stockExists)
            {
                throw new InvalidOperationException($"A stock with the symbol {request.Symbol} already exists.");
            }

            stock.Symbol = request.Symbol;
        }

        if (request.CurrentValue is not null)
        {
            stock.SetValue(request.CurrentValue);
        }

        if (request.RawDescription is not null)
        {
            stock.SetDescription(request.RawDescription, _formatter.Format(request.RawDescription));
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
