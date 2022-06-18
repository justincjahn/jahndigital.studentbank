using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Roles.Queries.GetRole;

public record GetRoleQuery(long Id) : IRequest<IQueryable<Role>>;

public class GetRoleQueryHandler : IRequestHandler<GetRoleQuery, IQueryable<Role>>
{
    private readonly IAppDbContext _context;

    public GetRoleQueryHandler(IAppDbContext context) {
        _context = context;
    }

    public Task<IQueryable<Role>> Handle(GetRoleQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_context.Roles.Where(x => x.Id == request.Id));
    }
}
