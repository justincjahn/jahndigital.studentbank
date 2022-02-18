using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Stocks.Queries.GetStocks;

public record GetStocksQuery(IEnumerable<long>? Instances, bool OnlyDeleted = false) : IRequest<IQueryable<Stock>>;

public class GetStocksQueryHandler : IRequestHandler<GetStocksQuery, IQueryable<Stock>>
{
    private readonly IAppDbContext _context;

    public GetStocksQueryHandler(IAppDbContext context)
    {
        _context = context;
    }
    
    public Task<IQueryable<Stock>> Handle(GetStocksQuery request, CancellationToken cancellationToken)
    {
        var query = (request.OnlyDeleted)
            ? _context.Stocks.Where(x => x.DateDeleted != null)
            : _context.Stocks.Where(x => x.DateDeleted == null);

        if (request.Instances is not null)
        {
            query = query.Where(x => x.StockInstances.Any(y => request.Instances.Contains(y.InstanceId)));
        }
        
        return Task.FromResult(query);
    }
}
