using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.ShareTypes.Commands.UnlinkShareTypeCommand;

public record UnlinkShareTypeCommand(long ShareTypeId, long InstanceId) : IRequest;

public class UnlinkShareTypeCommandHandler : IRequestHandler<UnlinkShareTypeCommand>
{
    private readonly IAppDbContext _context;

    public UnlinkShareTypeCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task<Unit> Handle(UnlinkShareTypeCommand request, CancellationToken cancellationToken)
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

        var link = await _context.ShareTypeInstances
            .Where(x => x.ShareTypeId == request.ShareTypeId && x.InstanceId == request.InstanceId)
            .FirstOrDefaultAsync(cancellationToken);

        if (link is null)
        {
            throw new NotFoundException(
                nameof(ShareTypeInstance),
                new object[] { request.ShareTypeId, request.InstanceId }
            );
        }

        bool hasShareInInstance = await _context.Shares
            .Include(x => x.Student)
            .ThenInclude(x => x.Group)
            .Where(x =>
                x.Student.Group.InstanceId == request.InstanceId
                && x.ShareTypeId == request.ShareTypeId
                && x.DateDeleted == null)
            .AnyAsync(cancellationToken);

        if (hasShareInInstance)
        {
            throw new InvalidOperationException(
                "There are still open shares of this share type in the instance you are trying to unlink!"
            );
        }

        _context.ShareTypeInstances.Remove(link);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
