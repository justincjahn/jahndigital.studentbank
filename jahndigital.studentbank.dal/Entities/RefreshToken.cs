using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using jahndigital.studentbank.dal.Attributes;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.dal.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [Owned]
    public class RefreshToken
    {
        /// <summary>
        /// Get or set the unique ID of this record.
        /// </summary>
        [Key]
        public long Id {get; set;} = default!;

        /// <summary>
        /// 
        /// </summary>
        [MaxLength(7168)]
        public string Token {get; set;} = null!;

        /// <summary>
        /// The date the token was created.
        /// </summary>
        [DateTimeKind(DateTimeKind.Utc)]
        public DateTime Created {get; set;}

        /// <summary>
        /// The date the token expires.
        /// </summary>
        [DateTimeKind(DateTimeKind.Utc)]
        public DateTime Expires {get; set;}

        /// <summary>
        /// The date the token was revoked (if any).
        /// </summary>
        [DateTimeKind(DateTimeKind.Utc)]
        public DateTime? Revoked {get; set;}

        /// <summary>
        /// If the token is expired.
        /// </summary>
        [NotMapped]
        public bool IsExpired => DateTime.UtcNow >= Expires;

        /// <summary>
        /// The IP address that created the token.
        /// </summary>
        [MaxLength(39)]
        public string CreatedByIpAddress {get; set;} = default!;

        /// <summary>
        /// The IP address that revoked the certificate.
        /// </summary>
        [MaxLength(39)]
        public string? RevokedByIpAddress {get; set;} = default!;

        /// <summary>
        /// The token that replaced this one.
        /// </summary>
        [MaxLength(7168)]
        public string? ReplacedByToken {get; set;} = default!;

        /// <summary>
        /// 
        /// </summary>
        [NotMapped]
        public bool IsActive => Revoked == null && !IsExpired;
    }
}
