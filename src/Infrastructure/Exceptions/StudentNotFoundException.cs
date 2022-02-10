namespace JahnDigital.StudentBank.Infrastructure.Exceptions;

public class StudentNotFoundException : DatabaseException
{
    public StudentNotFoundException(long studentId) : base(
        $"Student with ID {studentId} not found."
    )
    {
        StudentId = studentId;
    }

    public long StudentId { get; }
}
