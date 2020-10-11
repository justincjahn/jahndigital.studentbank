using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.dal.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [Owned]
    public class RefreshToken
    {
        // <summary>
        /// 
        /// </summary>
        public string Token {get; set;} = null!;

        /// <summary>
        /// The date the token was created.
        /// </summary>
        public DateTime Created {get; set;}

        /// <summary>
        /// The date the token expires.
        /// </summary>
        public DateTime Expires {get; set;}

        /// <summary>
        /// The date the token was revoked (if any).
        /// </summary>
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
        public string RevokedByIpAddress {get; set;} = default!;

        /// <summary>
        /// The token that replaced this one.
        /// </summary>
        public string ReplacedByToken {get; set;} = default!;

        /// <summary>
        /// 
        /// </summary>
        [NotMapped]
        public bool IsActive => Revoked == null && !IsExpired;
    }
}
