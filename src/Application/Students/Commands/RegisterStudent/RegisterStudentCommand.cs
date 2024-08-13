using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Students.Commands.RegisterStudent;

public record RegisterStudentCommand(long Id, DateTime DateRegistered, string Password, string? Email) : IRequest;

public class RegisterStudentCommandHandler : IRequestHandler<RegisterStudentCommand>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _hasher;

    public RegisterStudentCommandHandler(IAppDbContext context, IPasswordHasher hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    public async Task Handle(RegisterStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken)
        ?? throw new NotFoundException(nameof(Student), request.Id);

        if (student.DateRegistered is not null)
        {
            throw new UnauthorizedAccessException("The current user is not authorized to access this resource.");
        }

        if (request.Email is not null)
        {
            var emailExists = await _context
                .Instances
                .Where(x => x.IsActive)
                .Where(x => x.Groups.Any(g => g.Students.Any(y => y.Email == request.Email && y.Id != request.Id)))
            .AnyAsync(cancellationToken);

            if (emailExists)
            {
                throw new InvalidOperationException(
                    "A student with that email address already exists in an active instance.");
            }

            student.Email = request.Email;
        }

        student.DateRegistered = request.DateRegistered;
        student.Password = _hasher.HashPassword(request.Password);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
