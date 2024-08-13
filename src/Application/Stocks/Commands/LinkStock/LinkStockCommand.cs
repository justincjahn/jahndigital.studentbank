using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Stocks.Commands.LinkStock;

public record LinkStockCommand(long StockId, long InstanceId) : IRequest;

public class LinkStockCommandHandler : IRequestHandler<LinkStockCommand>
{
    private readonly IAppDbContext _context;

    public LinkStockCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task Handle(LinkStockCommand request, CancellationToken cancellationToken)
    {
        var hasInstance = await _context.Instances.AnyAsync(x => x.Id == request.InstanceId, cancellationToken);

        if (!hasInstance)
        {
            throw new NotFoundException(nameof(Instance), request.InstanceId);
        }

        var hasStock = await _context.Stocks.AnyAsync(x => x.Id == request.StockId, cancellationToken);

        if (!hasStock)
        {
            throw new NotFoundException(nameof(Stock), request.StockId);
        }

        bool hasLinks = await _context.StockInstances
            .Where(x => x.StockId == request.StockId && x.InstanceId == request.InstanceId)
            .AnyAsync(cancellationToken);

        if (hasLinks)
        {
            throw new InvalidOperationException("Stock is already linked to the provided instance.");
        }

        var link = new StockInstance { StockId = request.StockId, InstanceId = request.InstanceId };

        _context.StockInstances.Add(link);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
