using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace jahndigital.studentbank.server.Entities
{
    /// <summary>
    /// Represents a student
    /// </summary>
    public class Student
    {
        /// <summary>
        /// The unique ID of the student.
        /// </summary>
        public long Id {get; set;}

        /// <summary>
        /// Student's account number. Left-zero-fill to 10 characters.
        /// </summary>
        [MaxLength(10), MinLength(10), Required]
        public string AccountNumber {get; set;}

        /// <summary>
        /// Backing field for encrypted password.
        /// </summary>
        private string _password;

        /// <summary>
        /// The encrypted credentials of the user.
        /// </summary>
        [Column("Password"), MaxLength(84), Required]
        public string Password
        {
            get => _password;

            set
            {
                var passwordHasher = new PasswordHasher<Student>();
                _password = passwordHasher.HashPassword(this, value);
            }
        }

        /// <summary>
        /// Validate the provided password.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public PasswordVerificationResult ValidatePassword(string password)
        {
            var passwordHasher = new PasswordHasher<Student>();
            return passwordHasher.VerifyHashedPassword(this, Password, password);
        }

        /// <summary>
        /// Student's group (class/period/etc.).
        /// </summary>
        [Required]
        public Group Group { get; set; }
    }
}
