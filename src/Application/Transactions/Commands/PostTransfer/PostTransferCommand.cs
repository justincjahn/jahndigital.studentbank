using JahnDigital.StudentBank.Application.Transactions.DTOs;
using JahnDigital.StudentBank.Application.Transactions.Services;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Transactions.Commands.PostTransfer;

public class PostTransferCommandHandler : IRequestHandler<TransferRequest, (Transaction, Transaction)>
{
    private readonly ITransactionService _transactionService;

    public PostTransferCommandHandler(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public Task<(Transaction, Transaction)> Handle(TransferRequest request, CancellationToken cancellationToken)
    {
        return _transactionService.TransferAsync(request, cancellationToken);
    }
}
