using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using jahndigital.studentbank.server.Services;

namespace jahndigital.studentbank.server.Entities
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
        [Required]
        public char TransactionType {get; set;}

        /// <summary>
        /// The target share of the transaction.
        /// </summary>
        [Required]
        public Share TargetShare {get; set;}

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
            get
            {
                return Money.FromDatabase(RawAmount);
            }

            set
            {
                RawAmount = value.DatabaseAmount;
            }
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
            get
            {
                return Money.FromDatabase(RawNewBalance);
            }

            set
            {
                RawNewBalance = value.DatabaseAmount;
            }
        }

        /// <summary>
        /// The date the transaction was posted.
        /// </summary>
        [Required]
        public DateTime EffectiveDate {get; set;} = DateTime.UtcNow;
    }
}
