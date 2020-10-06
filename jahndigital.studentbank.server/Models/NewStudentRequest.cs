namespace jahndigital.studentbank.server.Models
{
    #nullable disable

    /// <summary>
    /// Data fields for a new student.
    /// </summary>
    public class NewStudentRequest
    {
        /// <summary>
        /// Get or set the student's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Get or set the student's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Get or set the student's account number.
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Get or set the student's email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Get or set the student's password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Get or set the student's group ID.
        /// </summary>
        public long GroupId { get; set; }
    }
}
