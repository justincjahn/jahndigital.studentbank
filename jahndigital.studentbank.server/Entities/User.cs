using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace jahndigital.studentbank.server.Entities
{
    /// <summary>
    /// Represents a backend user of the application (administrators)
    /// </summary>
    public class User
    {
        /// <summary>
        /// The unique ID of the user.
        /// </summary>
        /// <value></value>
        public long Id { get; set; }

        /// <summary>
        /// The email address of the user.
        /// </summary>
        /// <value></value>
        [MaxLength(64), Required]
        public string Email {get; set;}

        /// <summary>
        /// Backing field for encrypted password.
        /// </summary>
        private string _password = "";

        /// <summary>
        /// The encrypted credentials of the user.
        /// </summary>
        [Column("Password"), MaxLength(84), Required, JsonIgnore]
        public string Password {
            get => _password;

            set
            {
                var passwordHasher = new PasswordHasher<User>();
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
            var passwordHasher = new PasswordHasher<User>();
            return passwordHasher.VerifyHashedPassword(this, Password, password);
        }

        /// <summary>
        /// The user's role.
        /// </summary>
        [Required]
        public Role Role {get; set;}

        /// <summary>
        /// A list of JWT refresh tokens for the user.
        /// </summary>
        [JsonIgnore]
        public List<RefreshToken> RefreshTokens {get;set;}
    }
}
