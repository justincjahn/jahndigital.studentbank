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
    
    public async Task<Unit> Handle(RegisterStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _context.Students.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(Student), request.Id);

        student.DateRegistered = request.DateRegistered;
        student.Password = _hasher.HashPassword(request.Password);

        if (request.Email is not null)
        {
            student.Email = request.Email;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
