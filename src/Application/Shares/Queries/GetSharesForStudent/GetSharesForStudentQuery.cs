using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Shares.Queries.GetSharesForStudent;

/// <summary>
/// Get a query that returns a list of active or deleted shares for a given student.
/// </summary>
/// <param name="StudentId"></param>
public record GetSharesForStudent(long StudentId, bool OnlyDeleted = false) : IRequest<IQueryable<Share>>;

public class GetSharesForStudentQueryHandler : IRequestHandler<GetSharesForStudent, IQueryable<Share>>
{
    private readonly IAppDbContext _context;

    public GetSharesForStudentQueryHandler(IAppDbContext context)
    {
        _context = context;
    }
    
    public Task<IQueryable<Share>> Handle(GetSharesForStudent request, CancellationToken cancellationToken)
    {
        var query = _context
            .Shares
            .Where(x => x.StudentId == request.StudentId);

        query = (request.OnlyDeleted)
            ? query.Where(x => x.DateDeleted != null)
            : query.Where(x => x.DateDeleted == null);
            
        return Task.FromResult(query);
    }
}
