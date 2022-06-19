using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Products.Queries.GetProduct;

public record GetProductQuery(long Id) : IRequest<IQueryable<Product>>;

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, IQueryable<Product>>
{
    private readonly IAppDbContext _context;

    public GetProductQueryHandler(IAppDbContext context) {
        _context = context;
    }

    public Task<IQueryable<Product>> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_context.Products.Where(x => x.Id == request.Id && x.DateDeleted == null));
    }
}
