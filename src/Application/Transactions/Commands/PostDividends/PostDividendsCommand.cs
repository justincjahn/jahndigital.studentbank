using JahnDigital.StudentBank.Application.Transactions.DTOs;
using JahnDigital.StudentBank.Application.Transactions.Services;
using MediatR;

namespace JahnDigital.StudentBank.Application.Transactions.Commands.PostDividends;

public class PostDividendsCommandHandler : IRequestHandler<PostDividendsRequest>
{
    private readonly ITransactionService _transactionService;

    public PostDividendsCommandHandler(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public async Task<Unit> Handle(PostDividendsRequest request, CancellationToken cancellationToken)
    {
        await _transactionService.PostDividendsAsync(request, cancellationToken);
        return Unit.Value;
    }
}
