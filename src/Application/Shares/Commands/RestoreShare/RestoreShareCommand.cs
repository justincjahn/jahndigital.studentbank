using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Shares.Commands.RestoreShare;

public record RestoreShareCommand(long ShareId) : IRequest;

public class RestoreShareCommandHandler : IRequestHandler<RestoreShareCommand>
{
    private readonly IAppDbContext _context;

    public RestoreShareCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task Handle(RestoreShareCommand request, CancellationToken cancellationToken)
    {
        var share = await _context.Shares.FindAsync(new object?[] { request.ShareId }, cancellationToken)
            ?? throw new NotFoundException(nameof(Share), request.ShareId);

        share.DateDeleted = null;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
