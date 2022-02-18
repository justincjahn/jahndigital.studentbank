using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Stocks.Queries.GetStocksForStudent;

public record GetStocksForStudentQuery(long StudentId, bool OnlyDeleted = false) : IRequest<IQueryable<Stock>>;

public class GetStocksForStudentQueryHandler : IRequestHandler<GetStocksForStudentQuery, IQueryable<Stock>>
{
    private readonly IAppDbContext _context;

    public GetStocksForStudentQueryHandler(IAppDbContext context)
    {
        _context = context;
    }
    
    public async Task<IQueryable<Stock>> Handle(GetStocksForStudentQuery request, CancellationToken cancellationToken)
    {
        var availableStocks = await _context
            .Students
            .Include(x => x.Group)
            .ThenInclude(x => x.Instance)
            .ThenInclude(x => x.StockInstances)
            .Where(x => x.Id == request.StudentId)
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new StudentNotFoundException(request.StudentId);

        var stockIds = availableStocks
            .Group
            .Instance
            .StockInstances
            .Select(x => x.StockId);

        var query = (request.OnlyDeleted)
            ? _context.Stocks.Where(x => x.DateDeleted != null && stockIds.Contains(x.Id))
            : _context.Stocks.Where(x => x.DateDeleted == null && stockIds.Contains(x.Id));

        return query;
    }
}
