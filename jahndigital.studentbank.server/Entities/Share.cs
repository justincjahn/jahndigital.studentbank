using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using jahndigital.studentbank.server.Services;

namespace jahndigital.studentbank.server.Entities
{
    /// <summary>
    /// Represents a deposit product with a balance.
    /// </summary>
    public class Share
    {
        /// <summary>
        /// The unique ID of the share.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The Share Type information for this product.
        /// </summary>
        [Required]
        public ShareType ShareType { get; set; }

        /// <summary>
        /// The owner of the share.
        /// </summary>
        [Required]
        public Student Student {get; set;}

        /// <summary>
        /// 
        /// </summary>
        [Column("Balance"), Required]
        public long RawBalance { get; private set; } = 0;

        /// <summary>
        /// The current balance of the share.
        /// </summary>
        [NotMapped]
        public Money Balance {
            get
            {
                return Money.FromDatabase(RawBalance);
            }

            set
            {
                RawBalance = value.DatabaseAmount;
            }
        }

        /// <summary>
        /// The last activity date of the share.
        /// </summary>
        [Required]
        public DateTime DateLastActive {get; set;} = DateTime.UtcNow;
    }
}
