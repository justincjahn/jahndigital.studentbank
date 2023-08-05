using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Students.Commands.UpdateStudent;

public record UpdateStudentCommand : IRequest<Student>
{
    public long Id { get; init; }

    public string? AccountNumber { get; init; }

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public string? Email { get; init; }

    public string? Password { get; init; }
}

public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, Student>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _hasher;

    public UpdateStudentCommandHandler(IAppDbContext context, IPasswordHasher hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    public async Task<Student> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        Student student = await _context.Students.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Student), request.Id);

        if (request.AccountNumber is not null)
        {
            var studentExists = await _context.Instances
                .Where(x => x.Groups.Any(g => g.Id == student.GroupId)) // Limit query to student's instance
                .Where(x => x.Groups.Any(g =>
                    g.Students.Any(y =>
                        EF.Functions.Like(y.AccountNumber, $"{request.AccountNumber}")
                        && y.Id != student.Id
                )))
                .AnyAsync(cancellationToken);

            if (studentExists)
            {
                throw new InvalidOperationException(
                    $@"Provided Account Number {request.AccountNumber} already exists in the same instance.");
            }

            student.AccountNumber = request.AccountNumber;
        }

        if (request.FirstName is not null)
        {
            student.FirstName = request.FirstName;
        }

        if (request.LastName is not null)
        {
            student.LastName = request.LastName;
        }
        
        if (request.Password is not null)
        {
            student.Password = _hasher.HashPassword(request.Password);
        }

        if (request.Email is not null)
        {
            student.Email = request.Email;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return student;
    }
}
