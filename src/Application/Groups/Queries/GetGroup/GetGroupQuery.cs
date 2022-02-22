using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Groups.Queries.GetGroup;

public record GetGroupQuery(long GroupId, bool IncludeDeleted = false) : IRequest<IQueryable<Group>>;

public class GetGroupQueryHandler : IRequestHandler<GetGroupQuery, IQueryable<Group>>
{
    private readonly IAppDbContext _context;

    public GetGroupQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public Task<IQueryable<Group>> Handle(GetGroupQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            (request.IncludeDeleted)
                ? _context.Groups.Where(x => x.Id == request.GroupId)
                : _context.Groups.Where(x => x.Id == request.GroupId && x.DateDeleted == null)
        );
    }
}
