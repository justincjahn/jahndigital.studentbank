using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.ShareTypes.Queries.GetShareTypesForStudent;

public record GetShareTypesForStudentQuery(long StudentId, bool OnlyDeleted = false) : IRequest<IQueryable<ShareType>>;

public class GetShareTypesForStudentQueryHandler : IRequestHandler<GetShareTypesForStudentQuery, IQueryable<ShareType>>
{
    private readonly IAppDbContext _context;

    public GetShareTypesForStudentQueryHandler(IAppDbContext context)
    {
        _context = context;
    }
    
    public async Task<IQueryable<ShareType>> Handle(GetShareTypesForStudentQuery request, CancellationToken cancellationToken)
    {
        var shares = await _context
                .Students
                .Include(x => x.Group)
                .ThenInclude(x => x.Instance)
                .ThenInclude(x => x.ShareTypeInstances)
                .Where(x => x.Id == request.StudentId)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken)
        ?? throw new StudentNotFoundException(request.StudentId);

        var shareTypeIds = shares
            .Group
            .Instance
            .ShareTypeInstances
            .Select(x => x.ShareTypeId);

        var query = (request.OnlyDeleted)
            ? _context.ShareTypes.Where(x => x.DateDeleted != null && shareTypeIds.Contains(x.Id))
            : _context.ShareTypes.Where(x => x.DateDeleted == null && shareTypeIds.Contains(x.Id));

        return query;
    }
}
