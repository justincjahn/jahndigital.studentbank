using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Users.Commands.RestoreUser;

public record RestoreUserCommand(long Id) : IRequest;

public class RestoreUserCommandHandler : IRequestHandler<RestoreUserCommand>
{
    private readonly IAppDbContext _context;

    public RestoreUserCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task<Unit> Handle(RestoreUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object?[] { request.Id }, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        if (user.DateDeleted is null)
        {
            throw new InvalidOperationException($"User {request.Id} is not deleted.");
        }

        user.DateDeleted = null;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
