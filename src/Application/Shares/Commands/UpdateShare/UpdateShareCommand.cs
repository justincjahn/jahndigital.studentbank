using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Shares.Commands.UpdateShare;

public record UpdateShareCommand(long ShareId, long ShareTypeId) : IRequest;

public class UpdateShareCommandHandler : IRequestHandler<UpdateShareCommand>
{
    private readonly IAppDbContext _context;

    public UpdateShareCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateShareCommand request, CancellationToken cancellationToken)
    {
        var share = await _context.Shares.Where(x => x.Id == request.ShareId).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Share), request.ShareId);

        var student = await _context.Students
            .Include(x => x.Group)
            .ThenInclude(x => x.Instance)
            .ThenInclude(x => x.ShareTypeInstances)
            .Where(x =>
                x.Group.Instance.ShareTypeInstances.Any(y => y.ShareTypeId == request.ShareTypeId)
                && x.Id == share.StudentId)
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException(nameof(ShareType), request.ShareTypeId);

        share.ShareTypeId = request.ShareTypeId;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
