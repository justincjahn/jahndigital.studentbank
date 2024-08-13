using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.ShareTypes.Commands.DeleteShareType;

public record DeleteShareTypeCommand(long ShareTypeId) : IRequest;

public class DeleteShareTypeCommandHandler : IRequestHandler<DeleteShareTypeCommand>
{
    private readonly IAppDbContext _context;

    public DeleteShareTypeCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task Handle(DeleteShareTypeCommand request, CancellationToken cancellationToken)
    {
        var shareType = await _context.ShareTypes.FindAsync(new object?[] { request.ShareTypeId }, cancellationToken)
            ?? throw new NotFoundException(nameof(ShareType), request.ShareTypeId);

        bool hasShares = await _context.Shares.AnyAsync(
            x => x.ShareTypeId == request.ShareTypeId && x.DateDeleted == null, cancellationToken);

        if (hasShares)
        {
            throw new InvalidOperationException("Cannot delete a share type with active shares.");
        }

        shareType.DateDeleted = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
