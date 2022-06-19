using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Groups.Commands.UpdateGroup;

/**
 * Update and soft-delete/undelete a group.
 */
public record UpdateGroupCommand(long Id, long? InstanceId, string? Name, bool Deleted = false) : IRequest<Unit>;

public class UpdateGroupCommandHandler : IRequestHandler<UpdateGroupCommand>
{
    private readonly IAppDbContext _context;

    public UpdateGroupCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.Groups.SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Group), request.Id);

        if (request.InstanceId is not null)
        {
            var instanceExists = await _context
                .Instances
                .AnyAsync(x => x.Id == request.InstanceId && x.DateDeleted == null, cancellationToken);

            if (!instanceExists)
            {
                throw new NotFoundException(nameof(Instance), request.InstanceId);
            }

            group.InstanceId = request.InstanceId.Value;
        }

        if (request.Name is not null)
        {
            var groups = await _context
                .Groups
                .AnyAsync(x =>
                    x.InstanceId == group.InstanceId
                    && x.Id != group.Id
                    && x.Name.ToLower() == request.Name!.ToLower(),
                cancellationToken);

            if (groups)
            {
                throw new ArgumentException(
                    $"A Group named {request.Name} already exists in instance {group.InstanceId}."
                );
            }

            group.Name = request.Name;
        }

        if (request.Deleted)
        {
            var hasStudents = await _context
                .Students
                .AnyAsync(x => x.GroupId == request.Id, cancellationToken);

            if (hasStudents)
            {
                throw new InvalidOperationException(
                    $"{group.Name} cannot be deleted because it still contains students.  Please move them first."
                );
            }

            group.DateDeleted = DateTime.UtcNow;
        }

        if (!request.Deleted && group.DateDeleted is not null)
        {
            group.DateDeleted = null;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
