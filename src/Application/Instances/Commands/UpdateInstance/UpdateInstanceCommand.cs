using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Instances.Commands.UpdateInstance;

public record UpdateInstanceCommand(long Id, string? Description, bool? IsActive, bool Deleted = false) : IRequest;

public class UpdateInstanceCommandHandler : IRequestHandler<UpdateInstanceCommand>
{
    private readonly IAppDbContext _context;

    public UpdateInstanceCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateInstanceCommand request, CancellationToken cancellationToken)
    {
        var instance = await _context.Instances.FindAsync(new object?[] { request.Id }, cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(Instance));

        if (request.Description is not null)
        {
            bool hasName =
                await _context.Instances.AnyAsync(x => x.Description == request.Description && x.Id != request.Id, cancellationToken: cancellationToken);

            if (hasName)
            {
                throw new InvalidOperationException($"Instance with name '{request.Description}' already exists.");
            }

            instance.Description = request.Description;
        }

        // Only one instance may be active at a time
        if (request.IsActive == true)
        {
            foreach (var x in await _context.Instances.ToListAsync(cancellationToken: cancellationToken))
            {
                x.IsActive = false;
            }

            instance.IsActive = true;
        }

        if (request.Deleted)
        {
            var hasGroups =
                await _context.Groups.AnyAsync(x => x.InstanceId == request.Id && x.DateDeleted == null, cancellationToken);

            if (hasGroups)
            {
                throw new InvalidOperationException("Cannot delete an instance that still has groups!");
            }

            instance.DateDeleted = DateTime.UtcNow;
        }

        if (!request.Deleted && instance.DateDeleted is not null)
        {
            instance.DateDeleted = null;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
