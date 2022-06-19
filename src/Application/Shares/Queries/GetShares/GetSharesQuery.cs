using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Shares.Queries.GetShares;

/// <summary>
/// Get a query that returns either active or deleted shares.
/// </summary>
/// <param name="OnlyDeleted"></param>
public record GetSharesQuery(bool OnlyDeleted = false) : IRequest<IQueryable<Share>>;

public class GetSharesQueryHandler : IRequestHandler<GetSharesQuery, IQueryable<Share>>
{
    private readonly IAppDbContext _context;

    public GetSharesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }
    
    public Task<IQueryable<Share>> Handle(GetSharesQuery request, CancellationToken cancellationToken)
    {
        var query = (request.OnlyDeleted)
            ? _context.Shares.Where(x => x.DateDeleted == null)
            : _context.Shares.Where(x => x.DateDeleted != null);

        return Task.FromResult(query);
    }
}
