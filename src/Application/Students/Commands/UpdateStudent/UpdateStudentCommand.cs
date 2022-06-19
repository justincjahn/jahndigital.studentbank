using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Students.Commands.UpdateStudent;

public record UpdateStudentCommand(long Id, string? Email, string? Password) : IRequest<Student>;

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
