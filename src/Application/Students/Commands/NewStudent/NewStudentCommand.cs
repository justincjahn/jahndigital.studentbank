using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Students.Commands.NewStudent;

public record NewStudentCommand : IRequest<long>
{
    public long GroupId { get; init; }

    public string AccountNumber { get; init; } = default!;

    public string Password { get; init; } = default!;

    public string FirstName { get; init; } = default!;

    public string LastName { get; init; } = default!;

    public string? Email { get; init; } = null;
}

public class NewStudentCommandHandler : IRequestHandler<NewStudentCommand, long>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _hasher;

    public NewStudentCommandHandler(IAppDbContext context, IPasswordHasher hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    public async Task<long> Handle(NewStudentCommand request, CancellationToken cancellationToken)
    {
        var groupExists = await _context.Groups.AnyAsync(x => x.Id == request.GroupId, cancellationToken);

        if (!groupExists)
        {
            throw new NotFoundException(nameof(Group), request.GroupId);
        }

        var studentExists = await _context.Instances
            .Where(x => x.Groups.Any(g => g.Id == request.GroupId)) // Limit query to student's instance
            .Where(x => x.Groups.Any(g =>
                g.Students.Any(y =>
                    EF.Functions.Like(y.AccountNumber, $"%{request.AccountNumber}"))))
            .AnyAsync(cancellationToken);

        if (studentExists)
        {
            throw new InvalidOperationException(
                $"Provided Account Number {request.AccountNumber} already exists in the same instance.");
        }

        var student = new Student
        {
            AccountNumber = request.AccountNumber,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            GroupId = request.GroupId,
            Password = _hasher.HashPassword(request.Password)
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync(cancellationToken);

        return student.Id;
    }
}
