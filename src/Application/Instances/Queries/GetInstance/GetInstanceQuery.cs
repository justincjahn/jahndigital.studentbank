using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Instances.Queries.GetInstance;

public record GetInstanceQuery(long InstanceId, bool IncludeDeleted = false) : IRequest<IQueryable<Instance>>;

public class GetInstanceQueryHandler : IRequestHandler<GetInstanceQuery, IQueryable<Instance>>
{
    private readonly IAppDbContext _context;
    public GetInstanceQueryHandler(IAppDbContext context) {
        _context = context;
    }

    public Task<IQueryable<Instance>> Handle(GetInstanceQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            (request.IncludeDeleted)
                ? _context.Instances.Where(x => x.Id == request.InstanceId)
                : _context.Instances.Where(x => x.Id == request.InstanceId && x.DateDeleted == null)
        );
    }
}
