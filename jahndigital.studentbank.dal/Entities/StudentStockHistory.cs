using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using jahndigital.studentbank.dal.Attributes;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.dal.Entities
{
    /// <summary>
    /// Represents stock buy/sells over time for a specific Student.
    /// </summary>
    public class StudentStockHistory
    {
        /// <summary>
        /// The unique ID of this transaction.
        /// </summary>
        public long Id {get; set;}

        /// <summary>
        /// Get the ID number of the StudentStock.
        /// </summary>
        [ForeignKey("StudentStock")]
        public long StudentStockId {get; set;}

        /// <summary>
        /// Get the transaction ID
        /// </summary>
        [ForeignKey("Transaction")]
        public long TransactionId {get;set;}

        /// <summary>
        /// The stock the student currently owns.
        /// </summary>
        [Required]
        public StudentStock StudentStock {get; set;} = default!;

        /// <summary>
        /// The monetary transaction that occurred as a result of the buy/sell.
        /// </summary>
        /// <value></value>
        [Required]
        public Transaction Transaction {get; set;} = default!;

        /// <summary>
        /// The number of shares purchased or sold.
        /// </summary>
        [Required]
        public int Count {get; set;}

        /// <summary>
        /// Raw amount of each share at the time of purchase.
        /// </summary>
        [Column("Amount"), Required]
        public long RawAmount {get; private set;} = 0;

        /// <summary>
        /// The amount of each share at the time of purchase.
        /// </summary>
        [NotMapped]
        public Money Amount
        {
            get => Money.FromDatabase(RawAmount);
            set => RawAmount = value.DatabaseAmount;
        }

        /// <summary>
        /// The date the stock was bought/sold.
        /// </summary>
        [Required, DateTimeKind(DateTimeKind.Utc)]
        public DateTime DatePosted {get; set;} = DateTime.UtcNow;
    }
}
