using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Shares.Queries.GetShare;

public record GetShareQuery(long ShareId) : IRequest<IQueryable<Share>>;

public class GetShareQueryHandler : IRequestHandler<GetShareQuery, IQueryable<Share>>
{
    private readonly IAppDbContext _context;

    public GetShareQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public Task<IQueryable<Share>> Handle(GetShareQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_context.Shares.Where(x => x.Id == request.ShareId));
    }
}
