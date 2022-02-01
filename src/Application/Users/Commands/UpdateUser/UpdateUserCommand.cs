using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;

namespace JahnDigital.StudentBank.Application.Users.Commands.UpdateUser;

public record UpdateUserCommand(long Id, long? RoleId, string? Email, string? Password) : IRequest<User>;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, User>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _hasher;

    public UpdateUserCommandHandler(IAppDbContext context, IPasswordHasher hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    public async Task<User> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        User user = await _context.Users.FindAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        if (request.Password is not null && !_hasher.Validate(user.Password, request.Password))
        {
            user.Password = _hasher.HashPassword(request.Password);
        }

        if (request.Email is not null)
        {
            user.Email = request.Email;
        }

        if (request.RoleId is not null)
        {
            var role = await _context.Roles.FindAsync(request.RoleId, cancellationToken)
                ?? throw new NotFoundException(nameof(Role), request.RoleId);

            user.RoleId = role.Id;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return user;
    }
}
