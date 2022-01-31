namespace jahndigital.studentbank.server.Models
{
#nullable disable

    /// <summary>
    ///     Data fields to create a user.
    /// </summary>
    public class NewUserRequest
    {
        /// <summary>
        ///     Get or set the user's email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     Get or set the user's password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     Get or set the user's role.
        /// </summary>
        public long RoleId { get; set; }
    }
}
