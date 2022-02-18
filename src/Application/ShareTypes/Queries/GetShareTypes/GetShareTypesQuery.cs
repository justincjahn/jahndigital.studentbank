using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.ShareTypes.Queries.GetShareTypes;

public record GetShareTypesQuery(IEnumerable<long>? Instances, bool OnlyDeleted = false) : IRequest<IQueryable<ShareType>>;

public class GetShareTypesQueryHandler : IRequestHandler<GetShareTypesQuery, IQueryable<ShareType>>
{
    private readonly IAppDbContext _context;

    public GetShareTypesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }
    
    public Task<IQueryable<ShareType>> Handle(GetShareTypesQuery request, CancellationToken cancellationToken)
    {
        var query = (request.OnlyDeleted)
            ? _context.ShareTypes.Where(x => x.DateDeleted != null)
            : _context.ShareTypes.Where(x => x.DateDeleted == null);

        if (request.Instances is not null)
        {
            query = query.Where(x => x.ShareTypeInstances.Any(y => request.Instances.Contains(y.InstanceId)));
        }

        return Task.FromResult(query);
    }
}
