using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.dal.Entities
{
    /// <summary>
    /// Represents a monetary transaction on a Share.
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// Unique ID number for the transaction.
        /// </summary>
        public long Id {get; set;}

        /// <summary>
        /// Type of transaction.
        /// </summary>
        [MaxLength(1), Required]
        public string TransactionType {get; set;}

        /// <summary>
        /// Get the ID number of the target share.
        /// </summary>
        [ForeignKey("TargetShare")]
        public long TargetShareId {get; set;}

        /// <summary>
        /// The target share of the transaction.
        /// </summary>
        [Required]
        public Share TargetShare {get; set;}

        /// <summary>
        /// An optional transaction comment.
        /// </summary>
        [MaxLength(255)]
        public string Comment {get; set;}

        /// <summary>
        /// Raw dollar amount of the transaction.
        /// </summary>
        [Column("Amount"), Required]
        public long RawAmount {get; private set;}

        /// <summary>
        /// The dollar amount of the transaction.
        /// </summary>
        [NotMapped]
        public Money Amount
        {
            get => Money.FromDatabase(RawAmount);
            set => RawAmount = value.DatabaseAmount;
        }

        /// <summary>
        /// The raw new balance of the share.
        /// </summary>
        [Column("NewBalance"), Required]
        public long RawNewBalance {get; private set;}

        /// <summary>
        /// The new balance of the share.
        /// </summary>
        [NotMapped]
        public Money NewBalance
        {
            get => Money.FromDatabase(RawNewBalance);
            set => RawNewBalance = value.DatabaseAmount;
        }

        /// <summary>
        /// The date the transaction was posted.
        /// </summary>
        [Required]
        public DateTime EffectiveDate {get; set;} = DateTime.UtcNow;
    }
}
