using JahnDigital.StudentBank.Application.Transactions.DTOs;
using JahnDigital.StudentBank.Application.Transactions.Services;
using MediatR;

namespace JahnDigital.StudentBank.Application.StudentPurchases.Commands.NewPurchase;

/// <summary>
/// Attempt to make a purchase.
/// </summary>
public class NewPurchaseCommandHandler : IRequestHandler<PurchaseRequest, long>
{
    private readonly ITransactionService _transactionService;

    public NewPurchaseCommandHandler(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public async Task<long> Handle(PurchaseRequest request, CancellationToken cancellationToken)
    {
        var purchase = await _transactionService.PurchaseAsync(request, cancellationToken);
        return purchase.Id;
    }
}
