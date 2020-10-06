namespace jahndigital.studentbank.server.Models
{
    /// <summary>
    /// Data fields to update a user.
    /// </summary>
    public class UpdateUserRequest
    {
        /// <summary>
        /// Get or set the user's ID.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Get or set the user's email address.
        /// </summary>
        public string? Email {get; set;}

        /// <summary>
        /// Get or set the user's password.
        /// </summary>
        public string? Password {get; set;}

        /// <summary>
        /// Get or set the user's role.
        /// </summary>
        public long? RoleId {get; set;}
    }
}
