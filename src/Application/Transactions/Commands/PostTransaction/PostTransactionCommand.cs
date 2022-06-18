using JahnDigital.StudentBank.Application.Transactions.DTOs;
using JahnDigital.StudentBank.Application.Transactions.Services;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Transactions.Commands.PostTransaction;

public class PostTransactionCommandHandler : IRequestHandler<TransactionRequest, Transaction>
{
    private readonly ITransactionService _transactionService;

    public PostTransactionCommandHandler(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public Task<Transaction> Handle(TransactionRequest request, CancellationToken cancellationToken)
    {
        return _transactionService.PostAsync(request, cancellationToken: cancellationToken);
    }
}
