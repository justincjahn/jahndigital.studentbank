using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Stocks.Queries.GetStudentStockHistory;

public record GetStudentStockHistoryQuery(long StudentStockId) : IRequest<IQueryable<StudentStockHistory>>;

public class GetStudentStockHistoryQueryHandler : IRequestHandler<GetStudentStockHistoryQuery, IQueryable<StudentStockHistory>>
{
    private readonly IAppDbContext _context;

    public GetStudentStockHistoryQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public Task<IQueryable<StudentStockHistory>> Handle(GetStudentStockHistoryQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            _context
                .StudentStockHistory
                .Where(x => x.StudentStockId == request.StudentStockId)
        );
    }
}
