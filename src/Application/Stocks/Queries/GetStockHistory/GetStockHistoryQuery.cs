using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Stocks.Queries.GetStockHistory;

public record GetStockHistoryQuery(long StockId) : IRequest<IQueryable<StockHistory>>;

public class GetStockHistoryQueryHandler : IRequestHandler<GetStockHistoryQuery, IQueryable<StockHistory>>
{
    private readonly IAppDbContext _context;

    public GetStockHistoryQueryHandler(IAppDbContext context)
    {
        _context = context;
    }
    
    public Task<IQueryable<StockHistory>> Handle(GetStockHistoryQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult<IQueryable<StockHistory>>(
            _context
                .StockHistory
                .Where(x => x.StockId == request.StockId)
        );
    }
}
