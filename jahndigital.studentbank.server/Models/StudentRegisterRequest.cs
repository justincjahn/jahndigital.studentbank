namespace jahndigital.studentbank.server.Models
{
    public class StudentRegisterRequest
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}