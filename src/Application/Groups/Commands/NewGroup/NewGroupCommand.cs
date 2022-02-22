using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Groups.Commands.NewGroup;

public record NewGroupCommand(long InstanceId, string Name) : IRequest<long>;

public class NewGroupCommandHandler : IRequestHandler<NewGroupCommand, long>
{
    private readonly IAppDbContext _context;
    public NewGroupCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task<long> Handle(NewGroupCommand request, CancellationToken cancellationToken)
    {
        var instanceExists = await _context
            .Instances
            .AnyAsync(x => x.Id == request.InstanceId && x.DateDeleted == null, cancellationToken);

        if (!instanceExists)
        {
            throw new NotFoundException(nameof(Instance), request.InstanceId);
        }

        var groups = await _context
            .Groups
            .AnyAsync(x =>
                    x.InstanceId == request.InstanceId
                    && x.Name.ToLower() == request.Name!.ToLower(),
                cancellationToken);

        if (groups)
        {
            throw new ArgumentException(
                $"A Group named {request.Name} already exists in instance {request.InstanceId}."
            );
        }

        var group = new Group() { InstanceId = request.InstanceId, Name = request.Name };
        _context.Groups.Add(group);
        await _context.SaveChangesAsync(cancellationToken);
        return group.Id;
    }
}
