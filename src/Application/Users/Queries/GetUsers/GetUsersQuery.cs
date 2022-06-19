using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Application.Users.Queries.GetUser;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Users.Queries.GetUsers;

public record GetUsersQuery(bool OnlyDeleted = false) : IRequest<IQueryable<User>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IQueryable<User>>
{
    private readonly IAppDbContext _context;

    public GetUsersQueryHandler(IAppDbContext context) {
        _context = context;
    }

    public Task<IQueryable<User>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            (request.OnlyDeleted)
                ? _context.Users.Where(x => x.DateDeleted != null)
                : _context.Users.Where(x => x.DateDeleted == null)
        );
    }
}
