using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Products.Commands.UpdateProduct;

public record UpdateProductCommand : IRequest
{
    public long Id { get; init; }
    public string? Name { get; init; }
    public Money? Cost { get; init; }
    public bool? IsLimitedQuantity { get; init; }
    public int? Quantity { get; init; }
    public string? Description { get; init; }
    public bool Deleted { get; init; }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IAppDbContext _context;

    public UpdateProductCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product =
            await _context.Products.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken)
        ?? throw new NotFoundException(nameof(Product));

        product.Cost = request.Cost ?? product.Cost;
        product.IsLimitedQuantity = request.IsLimitedQuantity ?? product.IsLimitedQuantity;
        product.Quantity = !product.IsLimitedQuantity ? -1 : request.Quantity ?? product.Quantity;
        product.Description = request.Description ?? product.Description;

        if (request.Name is not null)
        {
            bool hasName =
                await _context.Products.AnyAsync(x => x.Id != product.Id && x.Name != request.Name, cancellationToken);

            if (hasName)
            {
                throw new InvalidOperationException($"Product with name '{request.Name}' already exists!");
            }

            product.Name = request.Name;
        }

        if (request.Deleted)
        {
            product.DateDeleted = DateTime.UtcNow;
        }

        if (!request.Deleted && product.DateDeleted is not null)
        {
            product.DateDeleted = null;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
