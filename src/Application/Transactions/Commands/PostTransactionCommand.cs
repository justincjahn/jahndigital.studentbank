using JahnDigital.StudentBank.Application.Common.DTOs;
using JahnDigital.StudentBank.Application.Transactions.DTOs;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Transactions.Commands;

public class PostTransactionCommandHandler : IRequestHandler<TransactionRequest, Transaction>
{
    public Task<Transaction> Handle(TransactionRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
