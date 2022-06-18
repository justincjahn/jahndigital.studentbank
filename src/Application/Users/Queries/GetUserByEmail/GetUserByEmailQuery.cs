using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Users.Queries.GetUserByEmail;

public record GetUserByEmailQuery(string Email) : IRequest<IQueryable<User>>;

public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, IQueryable<User>>
{
    private readonly IAppDbContext _context;

    public GetUserByEmailQueryHandler(IAppDbContext context) {
        _context = context;
    }

    public Task<IQueryable<User>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_context.Users.Where(x => x.Email == request.Email.ToLower()));
    }
}
