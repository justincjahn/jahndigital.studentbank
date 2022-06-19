using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Stocks.Queries.GetStudentStocks;

public record GetStudentStocksQuery(long StudentId) : IRequest<IQueryable<StudentStock>>;

public class GetStudentStocksQueryHandler : IRequestHandler<GetStudentStocksQuery, IQueryable<StudentStock>>
{
    private readonly IAppDbContext _context;

    public GetStudentStocksQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public Task<IQueryable<StudentStock>> Handle(GetStudentStocksQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_context.StudentStocks.Where(x => x.StudentId == request.StudentId));
    }
}
