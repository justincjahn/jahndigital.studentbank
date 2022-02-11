using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Students.Commands.AdminUpdateStudent;

public record AdminUpdateStudentCommand(long Id, string? AccountNumber, string? Email, string? FirstName,
    string? LastName, long? GroupId, string? Password) : IRequest;

public class AdminUpdateStudentCommandHandler : IRequestHandler<AdminUpdateStudentCommand>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _hasher;

    public AdminUpdateStudentCommandHandler(IAppDbContext context, IPasswordHasher hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    public async Task<Unit> Handle(AdminUpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _context.Students.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Student), request.Id);

        if (request.AccountNumber is not null)
        {
            student.AccountNumber = request.AccountNumber;
        }

        if (request.Email is not null)
        {
            student.Email = request.Email;
        }

        if (request.FirstName is not null)
        {
            student.FirstName = request.FirstName;
        }

        if (request.LastName is not null)
        {
            student.LastName = request.LastName;
        }

        if (request.GroupId is not null)
        {
            student.GroupId = request.GroupId.Value;
        }

        if (request.Password is not null)
        {
            student.Password = _hasher.HashPassword(request.Password);
        }

        await _context.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
