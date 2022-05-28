using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.ShareTypes.Commands.RestoreShareType;

public record RestoreShareTypeCommand(long ShareTypeId) : IRequest;

public class RestoreShareTypeCommandHandler : IRequestHandler<RestoreShareTypeCommand>
{
    private readonly IAppDbContext _context;

    public RestoreShareTypeCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task<Unit> Handle(RestoreShareTypeCommand request, CancellationToken cancellationToken)
    {
        var shareType = await _context.ShareTypes
            .Where(x => x.Id == request.ShareTypeId && x.DateDeleted != null)
            .SingleOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException(nameof(ShareType), request.ShareTypeId);

        shareType.DateDeleted = null;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
