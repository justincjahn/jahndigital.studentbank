using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Stocks.Commands.NewStock;

public record NewStockCommand(string Symbol, string Name, long TotalShares, Money CurrentValue) : IRequest<long>;

public class NewStockCommandHandler : IRequestHandler<NewStockCommand, long>
{
    private readonly IAppDbContext _context;

    public NewStockCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task<long> Handle(NewStockCommand request, CancellationToken cancellationToken)
    {
        bool hasStock = await _context.Stocks.AnyAsync(x => x.Symbol == request.Symbol, cancellationToken);

        if (hasStock)
        {
            throw new InvalidOperationException(
                $"A stock already exists with symbol '{request.Symbol}'!  Was it deleted?");
        }

        var stock = new Stock
        {
            Symbol = request.Symbol,
            Name = request.Name,
            CurrentValue = request.CurrentValue,
            TotalShares = request.TotalShares,
            AvailableShares = request.TotalShares
        };

        _context.Stocks.Add(stock);

        await _context.SaveChangesAsync(cancellationToken);

        return stock.Id;
    }
}
