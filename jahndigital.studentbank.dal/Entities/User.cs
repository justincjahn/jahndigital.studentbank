using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using jahndigital.studentbank.dal.Attributes;
using Microsoft.AspNetCore.Identity;

namespace jahndigital.studentbank.dal.Entities
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
        /// Foreign key for user's role.
        /// </summary>
        [ForeignKey("Role"), Required]
        public long RoleId {get; set;}

        /// <summary>
        /// The email address of the user.
        /// </summary>
        /// <value></value>
        [MaxLength(64), Required]
        public string Email {get; set;} = default!;

        /// <summary>
        /// The user's role.
        /// </summary>
        [Required]
        public Role Role {get; set;} = default!;

        /// <summary>
        /// Backing field for encrypted password.
        /// </summary>
        private string _password = "";

        /// <summary>
        /// The encrypted credentials of the user.
        /// </summary>
        [MaxLength(84), Required, JsonIgnore]
        public string Password {
            get => _password;
            set {
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
        /// A list of JWT refresh tokens for the user.
        /// </summary>
        [JsonIgnore]
        public ICollection<RefreshToken> RefreshTokens {get;set;} = new HashSet<RefreshToken>();

        /// <summary>
        /// Get or set the date the student last logged in.
        /// </summary>
        [DateTimeKind(DateTimeKind.Utc)]
        public DateTime? DateLastLogin { get; set; } = null;

        /// <summary>
        /// Get or set the date the user was created.
        /// </summary>
        [DateTimeKind(DateTimeKind.Utc)]
        public DateTime DateCreated {get; set;} = DateTime.UtcNow;

        /// <summary>
        /// Get or set the date the user was registered.
        /// </summary>
        [DateTimeKind(DateTimeKind.Utc)]
        public DateTime? DateRegistered { get; set; } = null;

        /// <summary>
        /// Get or set the date the user was deleted.
        /// </summary>
        [DateTimeKind(DateTimeKind.Utc)]
        public DateTime? DateDeleted {get; set;} = null;
    }
}
