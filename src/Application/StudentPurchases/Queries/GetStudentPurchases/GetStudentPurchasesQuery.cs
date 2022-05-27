using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.StudentPurchases.Queries.GetStudentPurchases;

public record GetStudentPurchasesQuery(long? StudentId = null) : IRequest<IQueryable<StudentPurchase>>;

public class GetStudentPurchasesQueryHandler : IRequestHandler<GetStudentPurchasesQuery, IQueryable<StudentPurchase>>
{
    private readonly IAppDbContext _context;

    public GetStudentPurchasesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public Task<IQueryable<StudentPurchase>> Handle(GetStudentPurchasesQuery request, CancellationToken cancellationToken)
    {
        if (!request.StudentId.HasValue) return Task.FromResult((IQueryable<StudentPurchase>)_context.StudentPurchases);

        return Task.FromResult(
            _context
                .StudentPurchases
                .Where(x => x.StudentId == request.StudentId)
        );
    }
}
