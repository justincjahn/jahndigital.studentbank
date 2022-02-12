namespace JahnDigital.StudentBank.Application.Common.Exceptions;

public class StudentNotFoundException : NotFoundException
{
    public StudentNotFoundException(long studentId) : base(
        $"Student with ID {studentId} not found."
    )
    {
        StudentId = studentId;
    }

    public long StudentId { get; }
}
