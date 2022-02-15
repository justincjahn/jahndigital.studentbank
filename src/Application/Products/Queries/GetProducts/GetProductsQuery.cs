using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Products.Queries.GetProducts;

/// <summary>
/// Get a query that returns either active or deleted products.
/// </summary>
/// <param name="OnlyDeleted"></param>
public record GetProductsQuery(bool OnlyDeleted = false) : IRequest<IQueryable<Product>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IQueryable<Product>>
{
    private readonly IAppDbContext _context;

    public GetProductsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }
    
    public Task<IQueryable<Product>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = (request.OnlyDeleted)
            ? _context.Products.Where(x => x.DateDeleted != null)
            : _context.Products.Where(x => x.DateDeleted == null);

        return Task.FromResult(query);
    }
}
