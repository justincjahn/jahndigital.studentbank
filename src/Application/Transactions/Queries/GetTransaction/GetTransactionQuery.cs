using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Transactions.Queries.GetTransaction;

/// <summary>
/// Fetch a specific transaction from the database.
/// </summary>
/// <param name="id"></param>
public record GetTransactionQuery(long id) : IRequest<IQueryable<Transaction>>;

public class GetTransactionQueryHandler : IRequestHandler<GetTransactionQuery, IQueryable<Transaction>>
{
    private readonly IAppDbContext _context;

    public GetTransactionQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public Task<IQueryable<Transaction>> Handle(GetTransactionQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_context.Transactions.Where(x => x.Id == request.id));
    }
}
