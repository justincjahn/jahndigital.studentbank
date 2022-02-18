using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Transactions.Queries.GetShareTransactions;

public record GetShareTransactionsQuery(long ShareId) : IRequest<IQueryable<Transaction>>;

public class GetShareTransactionsQueryHandler : IRequestHandler<GetShareTransactionsQuery, IQueryable<Transaction>>
{
    private readonly IAppDbContext _context;

    public GetShareTransactionsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public Task<IQueryable<Transaction>> Handle(GetShareTransactionsQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_context.Transactions.Where(x => x.TargetShareId == request.ShareId));
    }
}
