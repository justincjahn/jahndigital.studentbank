using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Students.Queries.GetStudents;

public record GetStudentsQuery(bool OnlyDeleted = false) : IRequest<IQueryable<Student>>;

public class GetStudentsQueryHandler : IRequestHandler<GetStudentsQuery, IQueryable<Student>>
{
    private readonly IAppDbContext _context;

    public GetStudentsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }
    
    public Task<IQueryable<Student>> Handle(GetStudentsQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            (request.OnlyDeleted)
                ? _context.Students.Where(x => x.DateDeleted != null)
                : _context.Students.Where(x => x.DateDeleted == null)
        );
    }
}
