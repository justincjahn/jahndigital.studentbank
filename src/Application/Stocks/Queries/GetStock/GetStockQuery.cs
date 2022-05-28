using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Stocks.Queries.GetStock;

public record GetStockQuery(long StockId) : IRequest<IQueryable<Stock>>;

public class GetStockQueryHandler : IRequestHandler<GetStockQuery, IQueryable<Stock>>
{
    private readonly IAppDbContext _context;

    public GetStockQueryHandler(IAppDbContext context) {
        _context = context;
    }

    public Task<IQueryable<Stock>> Handle(GetStockQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult<IQueryable<Stock>>(_context.Stocks.Where(x => x.Id == request.StockId));
    }
}
