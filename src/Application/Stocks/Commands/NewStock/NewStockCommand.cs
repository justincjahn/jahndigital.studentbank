using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Stocks.Commands.NewStock;

public record NewStockCommand(string Symbol, string Name, Money CurrentValue, String RawDescription = "") : IRequest<long>;

public class NewStockCommandHandler : IRequestHandler<NewStockCommand, long>
{
    private readonly IAppDbContext _context;
    private readonly ITextFormatter _formatter;

    public NewStockCommandHandler(IAppDbContext context, ITextFormatter formatter)
    {
        _context = context;
        _formatter = formatter;
    }

    public async Task<long> Handle(NewStockCommand request, CancellationToken cancellationToken)
    {
        bool hasStock = await _context.Stocks.AnyAsync(x => x.Symbol == request.Symbol, cancellationToken);

        if (hasStock)
        {
            throw new InvalidOperationException(
                $"A stock already exists with symbol '{request.Symbol}'!  Was it deleted?");
        }

        var stock = new Stock(request.Symbol, request.Name, request.CurrentValue, request.RawDescription, _formatter.Format(request.RawDescription));
        _context.Stocks.Add(stock);
        await _context.SaveChangesAsync(cancellationToken);

        return stock.Id;
    }
}
