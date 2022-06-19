using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.ShareTypes.Queries.GetShareType;

public record GetShareTypeQuery(long ShareTypeId) : IRequest<IQueryable<ShareType>>;

public class GetShareTypeQueryHandler : IRequestHandler<GetShareTypeQuery, IQueryable<ShareType>>
{
    private readonly IAppDbContext _context;

    public GetShareTypeQueryHandler(IAppDbContext context) {
        _context = context;
    }

    public Task<IQueryable<ShareType>> Handle(GetShareTypeQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult<IQueryable<ShareType>>(_context.ShareTypes.Where(x => x.Id == request.ShareTypeId));
    }
}
