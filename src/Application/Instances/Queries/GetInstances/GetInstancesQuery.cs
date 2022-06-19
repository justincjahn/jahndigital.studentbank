using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Instances.Queries.GetInstances;

/// <summary>
/// Get a query that returns either active or deleted instances.
/// </summary>
/// <param name="OnlyDeleted"></param>
public record GetInstancesQuery(bool OnlyDeleted = false) : IRequest<IQueryable<Instance>>;

public class GetInstancesQueryHandler : IRequestHandler<GetInstancesQuery, IQueryable<Instance>>
{
    private readonly IAppDbContext _context;

    public GetInstancesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }
    
    public Task<IQueryable<Instance>> Handle(GetInstancesQuery request, CancellationToken cancellationToken)
    {
        var query = (request.OnlyDeleted)
            ? _context.Instances.Where(x => x.DateDeleted != null)
            : _context.Instances.Where(x => x.DateDeleted == null);

        return Task.FromResult(query);
    }
}
