using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.ValueObjects;
using MediatR;

namespace JahnDigital.StudentBank.Application.Shares.Commands.DeleteShare;

public record DeleteShareCommand(long ShareId) : IRequest;

public class DeleteShareCommandHandler : IRequestHandler<DeleteShareCommand>
{
    private readonly IAppDbContext _context;

    public DeleteShareCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteShareCommand request, CancellationToken cancellationToken)
    {
        var share = await _context.Shares.FindAsync(new object?[] { request.ShareId }, cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(Share), request.ShareId);

        if (share.Balance != Money.Zero)
        {
            throw new InvalidOperationException("Share must be zero-balance before being deleted.");
        }

        share.DateDeleted = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
