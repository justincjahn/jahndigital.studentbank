using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Students.Queries.GetStudentByAccount;

public record GetStudentByAccountQuery(string AccountNumber, long? GroupId = null) : IRequest<IQueryable<Student>>;

public class GetStudentByAccountQueryHandler : IRequestHandler<GetStudentByAccountQuery, IQueryable<Student>>
{
    private readonly IAppDbContext _context;

    public GetStudentByAccountQueryHandler(IAppDbContext context) {
        _context = context;
    }

    public Task<IQueryable<Student>> Handle(GetStudentByAccountQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Students.Where(x =>
            EF.Functions.Like(x.AccountNumber, $"%{request.AccountNumber}"));

        if (request.GroupId is not null)
        {
            query = query.Where(x => x.GroupId == request.GroupId);
        }

        return Task.FromResult(query);
    }
}
