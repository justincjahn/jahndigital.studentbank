namespace jahndigital.studentbank.services.Exceptions
{
    public class StudentNotFoundException : BaseException
    {
        public long StudentId { get; }

        public StudentNotFoundException(long studentId) : base(
            $"Student with ID {studentId} not found."
        ) {
            StudentId = studentId;
        }
    }
}
