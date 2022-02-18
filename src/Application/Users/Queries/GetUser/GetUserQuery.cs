using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Users.Queries.GetUser;

public record GetUserQuery(long UserId, bool IncludeDeleted = false) : IRequest<IQueryable<User>>;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, IQueryable<User>>
{
    private readonly IAppDbContext _context;

    public GetUserQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public Task<IQueryable<User>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var query = (request.IncludeDeleted)
            ? _context.Users.Where(x => x.Id == request.UserId)
            : _context.Users.Where(x => x.Id == request.UserId && x.DateDeleted == null);

        return Task.FromResult(query);
    }
}
