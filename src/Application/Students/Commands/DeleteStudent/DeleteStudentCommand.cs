using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using MediatR;

namespace JahnDigital.StudentBank.Application.Students.Commands.DeleteStudent;

public record DeleteStudentCommand(long Id) : IRequest;

public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand>
{
    private readonly IAppDbContext _context;

    public DeleteStudentCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _context.Students.FindAsync(new object?[] { request.Id }, cancellationToken)
            ?? throw new StudentNotFoundException(request.Id);

        if (student.DateDeleted is not null)
        {
            throw new InvalidOperationException($"Student {request.Id} was already deleted.");
        }

        student.DateDeleted = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
