using System;
using System.Text.RegularExpressions;

namespace jahndigital.studentbank.server.Models
{
#nullable disable

    /// <summary>
    ///     Data fields for a new student.
    /// </summary>
    public class NewStudentRequest
    {
        private string _accountNumber;

        /// <summary>
        ///     Get or set the student's account number.
        /// </summary>
        public string AccountNumber
        {
            get => _accountNumber;

            set {
                if (!Regex.IsMatch(value, "^[0-9]{1,10}$")) {
                    throw new ArgumentOutOfRangeException(
                        "AccountNumber",
                        "Can be a maximum of 10 digits."
                    );
                }

                _accountNumber = value.PadLeft(10, '0');
            }
        }

        /// <summary>
        ///     Get or set the student's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        ///     Get or set the student's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        ///     Get or set the student's password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     Get or set the student's group ID.
        /// </summary>
        public long GroupId { get; set; }

#nullable enable

        /// <summary>
        ///     Get or set the student's email address.
        /// </summary>
        public string? Email { get; set; }
    }
}