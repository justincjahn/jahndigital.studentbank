using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Users.Commands.NewUser;

public record NewUserCommand(long RoleId, string Email, string Password) : IRequest<long>;

public class NewUserCommandHandler : IRequestHandler<NewUserCommand, long>
{
    private readonly IAppDbContext _context;

    public NewUserCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task<long> Handle(NewUserCommand request, CancellationToken cancellationToken)
    {
        bool roleExists = await _context.Roles.Where(x => x.Id == request.RoleId).AnyAsync(cancellationToken);

        if (!roleExists)
        {
            throw new NotFoundException(nameof(Role), request.RoleId);
        }

        bool userExists = await _context.Users.Where(x => x.Email == request.Email.ToLower())
            .AnyAsync(cancellationToken);

        if (userExists)
        {
            throw new InvalidOperationException($"A user with the email {request.Email} already exists.");
        }

        var user = new User() { RoleId = request.RoleId, Email = request.Email, Password = request.Password };
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user.Id;
    }
}
