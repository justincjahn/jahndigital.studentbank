using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.StudentPurchases.Queries.GetStudentPurchase;

public record GetStudentPurchaseQuery(long id) : IRequest<IQueryable<StudentPurchase>>;

public class GetStudentPurchaseQueryHandler : IRequestHandler<GetStudentPurchaseQuery, IQueryable<StudentPurchase>>
{
    private IAppDbContext _context;

    public GetStudentPurchaseQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public Task<IQueryable<StudentPurchase>> Handle(GetStudentPurchaseQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_context.StudentPurchases.Where(x => x.Id == request.id));
    }
}
