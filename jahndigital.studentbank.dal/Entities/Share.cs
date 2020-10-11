using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.dal.Entities
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
        /// Get the ShareType ID
        /// </summary>
        [ForeignKey("ShareType")]
        public long ShareTypeId {get; set;}

        /// <summary>
        /// The Share Type information for this product.
        /// </summary>
        [Required]
        public ShareType ShareType { get; set; }

        /// <summary>
        /// Get the Student ID
        /// </summary>
        [ForeignKey("Student")]
        public long StudentId {get;set;}

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
            get => Money.FromDatabase(RawBalance);
            set =>RawBalance = value.DatabaseAmount;
        }

        /// <summary>
        /// A list of transactions for the share.
        /// </summary>
        /// <typeparam name="Transaction"></typeparam>
        /// <returns></returns>
        public ICollection<Transaction> Transactions {get; set;} = new HashSet<Transaction>();

        /// <summary>
        /// The last activity date of the share.
        /// </summary>
        [Required]
        public DateTime DateLastActive {get; set;} = DateTime.UtcNow;

        /// <summary>
        /// Get the date the share was created.
        /// </summary>
        public DateTime DateCreated {get; private set;} = DateTime.UtcNow;

        /// <summary>
        /// Get or set the date the share was deleted.
        /// </summary>
        public DateTime? DateDeleted {get; set;} = null;
    }
}
