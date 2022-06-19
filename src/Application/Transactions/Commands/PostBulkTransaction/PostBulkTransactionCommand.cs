using JahnDigital.StudentBank.Application.Transactions.DTOs;
using JahnDigital.StudentBank.Application.Transactions.Services;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Transactions.Commands.PostBulkTransaction;

public record PostBulkTransactionCommand(IEnumerable<TransactionRequest> TransactionRequests, bool StopOnException = true, bool AssessWithdrawalLimit = true) : IRequest<IQueryable<Transaction>>;

public class PostBulkTransactionCommandHandler : IRequestHandler<PostBulkTransactionCommand, IQueryable<Transaction>>
{
    private readonly ITransactionService _transactionService;

    public PostBulkTransactionCommandHandler(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public Task<IQueryable<Transaction>> Handle(PostBulkTransactionCommand request, CancellationToken cancellationToken)
    {
        return _transactionService.PostAsync(
            request.TransactionRequests,
            request.StopOnException,
            request.AssessWithdrawalLimit,
            cancellationToken
        );
    }
}
