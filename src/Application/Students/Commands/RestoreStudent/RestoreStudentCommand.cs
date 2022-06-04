using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using MediatR;

namespace JahnDigital.StudentBank.Application.Students.Commands.RestoreStudent;

public record RestoreStudentCommand(long Id) : IRequest;

public class RestoreStudentCommandHandler : IRequestHandler<RestoreStudentCommand>
{
    private readonly IAppDbContext _context;

    public RestoreStudentCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task<Unit> Handle(RestoreStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _context.Students.FindAsync(new object?[] { request.Id }, cancellationToken)
            ?? throw new StudentNotFoundException(request.Id);

        if (student.DateDeleted is null)
        {
            throw new InvalidOperationException($"Student {request.Id} is not deleted.");
        }

        student.DateDeleted = null;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
