using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.ShareTypes.Commands.LinkShareTypeCommand;

public record LinkShareTypeCommand(long ShareTypeId, long InstanceId) : IRequest;

public class LinkShareTypeCommandHandler : IRequestHandler<LinkShareTypeCommand>
{
    private readonly IAppDbContext _context;

    public LinkShareTypeCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task<Unit> Handle(LinkShareTypeCommand request, CancellationToken cancellationToken)
    {
        bool hasInstance = await _context.Instances.AnyAsync(x => x.Id == request.InstanceId, cancellationToken);

        if (!hasInstance)
        {
            throw new NotFoundException(nameof(Instance), request.InstanceId);
        }

        bool hasShareType = await _context.ShareTypes.AnyAsync(x => x.Id == request.ShareTypeId, cancellationToken);

        if (!hasShareType)
        {
            throw new NotFoundException(nameof(ShareType), request.ShareTypeId);
        }

        bool hasLinks = await _context.ShareTypeInstances
            .Where(x => x.ShareTypeId == request.ShareTypeId && x.InstanceId == request.InstanceId)
            .AnyAsync(cancellationToken);

        if (hasLinks)
        {
            throw new InvalidOperationException("Share Type is already linked to the provided instance!");
        }

        var link = new ShareTypeInstance { ShareTypeId = request.ShareTypeId, InstanceId = request.InstanceId };

        _context.ShareTypeInstances.Add(link);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
