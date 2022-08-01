using FluentValidation;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Stocks.Commands.UpdateStock;

public class UpdateStockCommandValidator : AbstractValidator<UpdateStockCommand>
{
    public UpdateStockCommandValidator(IAppDbContext context)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(32)
            .WithMessage("Name must not exceed 32 characters.")
            .MinimumLength(3)
            .WithMessage("Name must be at least 3 characters.")
            .MustAsync(async (command, name, cancellationToken) => await StockExists(command.StockId, name, context, cancellationToken))
            .WithMessage(command => $"A stock named {command.Name} already exists.")
            .When(x => !String.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Symbol)
            .NotEmpty()
            .MaximumLength(10)
            .WithMessage("Symbol cannot exceed 10 characters.")
            .MinimumLength(3)
            .WithMessage("Symbol must be at least 3 characters.")
            .MustAsync(async (command, symbol, cancellationToken) => await SymbolExists(command.StockId, symbol, context, cancellationToken))
            .WithMessage(command => $"A stock with the symbol {command.Symbol} already exists.")
            .When(x => !String.IsNullOrWhiteSpace(x.Symbol));
    }

    /// <summary>
    /// Returns true if a stock with a different ID number has the same name.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task<bool> StockExists(long id, string name, IAppDbContext context, CancellationToken cancellationToken)
    {
        bool stockExists = await context.Stocks.Where(x => x.Name.ToLower() == name.ToLower() && x.Id != id)
            .AnyAsync(cancellationToken);

        return !stockExists;
    }

    /// <summary>
    /// Returns true if a stock with a different ID has the same symbol.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="symbol"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task<bool> SymbolExists(long id, string symbol, IAppDbContext context, CancellationToken cancellationToken)
    {
        bool symbolExists = await context.Stocks.Where(x => x.Symbol.ToLower() == symbol.ToLower() && x.Id != id)
            .AnyAsync(cancellationToken);

        return !symbolExists;
    }
}
