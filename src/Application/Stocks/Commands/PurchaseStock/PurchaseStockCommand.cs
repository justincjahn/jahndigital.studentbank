using JahnDigital.StudentBank.Application.Transactions.DTOs;
using JahnDigital.StudentBank.Application.Transactions.Services;
using MediatR;

namespace JahnDigital.StudentBank.Application.Stocks.Commands.PurchaseStock;

public class PurchaseStockCommandHandler : IRequestHandler<PurchaseStockRequest, long>
{
    private readonly ITransactionService _transactionService;

    public PurchaseStockCommandHandler(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public async Task<long> Handle(PurchaseStockRequest request, CancellationToken cancellationToken)
    {
        return (await _transactionService.PurchaseStockAsync(request, cancellationToken)).Id;
    }
}
