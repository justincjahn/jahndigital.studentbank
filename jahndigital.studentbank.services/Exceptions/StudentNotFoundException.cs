namespace jahndigital.studentbank.services.Exceptions
{
    public class StudentNotFoundException : BaseException
    {
        public StudentNotFoundException(long studentId) : base(
            $"Student with ID {studentId} not found."
        )
        {
            StudentId = studentId;
        }

        public long StudentId { get; }
    }
}