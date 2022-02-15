using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Groups.Queries.GetGroups;

/// <summary>
/// Get a query that returns either active or deleted groups.
/// </summary>
/// <param name="OnlyDeleted"></param>
public record GetGroupsQuery(bool OnlyDeleted = false) : IRequest<IQueryable<Group>>;

public class GetGroupsQueryHandler : IRequestHandler<GetGroupsQuery, IQueryable<Group>>
{
    private readonly IAppDbContext _context;

    public GetGroupsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }
    
    public Task<IQueryable<Group>> Handle(GetGroupsQuery request, CancellationToken cancellationToken)
    {
        var query = (request.OnlyDeleted)
            ? _context.Groups.Where(x => x.DateDeleted != null)
            : _context.Groups.Where(x => x.DateDeleted == null);

        return Task.FromResult(query);
    }
}
