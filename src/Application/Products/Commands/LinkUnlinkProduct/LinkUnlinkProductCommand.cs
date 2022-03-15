using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Products.Commands.LinkUnlinkProduct;

public record LinkUnlinkProductCommand(long ProductId, long InstanceId, bool Link) : IRequest;

public class LinkUnlinkProductCommandHandler : IRequestHandler<LinkUnlinkProductCommand>
{
    private readonly IAppDbContext _context;

    public LinkUnlinkProductCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task<Unit> Handle(LinkUnlinkProductCommand request, CancellationToken cancellationToken)
    {
        var link = await _context.ProductInstances.FirstOrDefaultAsync(x =>
            x.ProductId == request.ProductId && x.InstanceId == request.InstanceId, cancellationToken);

        if (request.Link)
        {
            if (link is not null)
            {
                throw new InvalidOperationException("A link already exists!");
            }

            var productInstance =
                new ProductInstance { ProductId = request.ProductId, InstanceId = request.InstanceId };

            _context.ProductInstances.Add(productInstance);
            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }

        if (link is null)
        {
            throw new NotFoundException(nameof(ProductInstance));
        }

        _context.ProductInstances.Remove(link);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
