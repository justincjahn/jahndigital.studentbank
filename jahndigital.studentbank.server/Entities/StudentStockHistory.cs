using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using jahndigital.studentbank.server.utils;

namespace jahndigital.studentbank.server.Entities
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
        /// The stock the student currently owns.
        /// </summary>
        [Required]
        public StudentStock StudentStock {get; set;}

        /// <summary>
        /// The number of shares purchased or sold.
        /// </summary>
        [Required]
        public int Count {get; set;}

        /// <summary>
        /// Raw amount of each share at the time of purchase.
        /// </summary>
        [Column("Amount"), Required]
        public long RawAmount { get; private set; } = 0;

        /// <summary>
        /// The amount of each share at the time of purchase.
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
        /// The monetary transaction that occurred as a result of the buy/sell.
        /// </summary>
        /// <value></value>
        [Required]
        public Transaction Transaction {get; set;}

        /// <summary>
        /// The date the stock was bought/sold.
        /// </summary>
        [Required]
        public DateTime DatePosted {get; set;} = DateTime.UtcNow;
    }
}
