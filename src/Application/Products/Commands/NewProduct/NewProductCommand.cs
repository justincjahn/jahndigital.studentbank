using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Products.Commands.NewProduct;

public record NewProductCommand(
    string Name,
    string Description,
    Money Cost,
    bool IsLimitedQuantity = false,
    int Quantity = -1
) : IRequest<long>;

public class NewProductCommandHandler : IRequestHandler<NewProductCommand, long>
{
    private readonly IAppDbContext _context;

    public NewProductCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task<long> Handle(NewProductCommand request, CancellationToken cancellationToken)
    {
        bool hasProduct = await _context.Products.AnyAsync(x => x.Name == request.Name, cancellationToken);

        if (hasProduct)
        {
            throw new InvalidOperationException($"A product with the name '{request.Name}' already exists!");
        }

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Cost = request.Cost,
            IsLimitedQuantity = request.IsLimitedQuantity,
            Quantity = !request.IsLimitedQuantity ? -1 : request.Quantity
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);
        return product.Id;
    }
}
