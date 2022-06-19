using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Stocks.Queries.GetStudentStock;

public record GetStudentStockQuery(long StudentStockId) : IRequest<IQueryable<StudentStock>>;

public class GetStudentStockQueryHandler : IRequestHandler<GetStudentStockQuery, IQueryable<StudentStock>>
{
    private readonly IAppDbContext _context;

    public GetStudentStockQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public Task<IQueryable<StudentStock>> Handle(GetStudentStockQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_context.StudentStocks.Where(x => x.Id == request.StudentStockId));
    }
}
