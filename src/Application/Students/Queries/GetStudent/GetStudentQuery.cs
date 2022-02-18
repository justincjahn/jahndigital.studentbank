using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Students.Queries.GetStudent;

public record GetStudentQuery(long StudentId, bool IncludeDeleted = false) : IRequest<IQueryable<Student>>;

public class GetStudentQueryHandler : IRequestHandler<GetStudentQuery, IQueryable<Student>>
{
    private readonly IAppDbContext _context;

    public GetStudentQueryHandler(IAppDbContext context)
    {
        _context = context;
    }
    
    public Task<IQueryable<Student>> Handle(GetStudentQuery request, CancellationToken cancellationToken)
    {
        var query = _context
            .Students
            .Where(x => x.Id == request.StudentId);

        if (!request.IncludeDeleted)
        {
            query = query.Where(x => x.DateDeleted == null);
        }

        return Task.FromResult(query);
    }
}
