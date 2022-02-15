using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Products.Queries.GetStudentProducts;

/// <summary>
/// Get a query that returns the product(s) a given student has access to.
/// </summary>
/// <param name="StudentId"></param>
public record GetStudentProducts(long StudentId) : IRequest<IQueryable<Product>>;

public class GetStudentProductsQueryHandler : IRequestHandler<GetStudentProducts, IQueryable<Product>>
{
    private readonly IAppDbContext _context;

    public GetStudentProductsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }
    
    public async Task<IQueryable<Product>> Handle(GetStudentProducts request, CancellationToken cancellationToken)
    {
        // Fetch the product IDs the user has access to
        Student student = await _context
                .Students
                .Include(x => x.Group)
                .ThenInclude(x => x.Instance)
                .ThenInclude(x => x.ProductInstances)
                .Where(x => x.Id == request.StudentId)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken)
            ?? throw new StudentNotFoundException(request.StudentId);

        var productIds = student
            .Group
            .Instance
            .ProductInstances
            .Select(x => x.ProductId);

        return _context.Products.Where(x => productIds.Contains(x.Id) && x.DateDeleted == null);
    }
}
