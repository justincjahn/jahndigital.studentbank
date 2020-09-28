using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using jahndigital.studentbank.server.utils;

namespace jahndigital.studentbank.server.Entities
{
    /// <summary>
    /// Represents the price history of a particular stock.
    /// </summary>
    public class StockHistory
    {
        /// <summary>
        /// Unique ID for this history entry.
        /// </summary>
        public long Id {get; set;}

        /// <summary>
        /// The stock.
        /// </summary>
        [Required]
        public Stock Stock {get; set;}

        /// <summary>
        /// Represents the raw value of the stock at the time.
        /// Use Value to set this value.
        /// </summary>
        [Column("Value"), Required]
        public long RawValue { get; private set; } = 0;

        /// <summary>
        /// The new value of the stock.
        /// </summary>
        [NotMapped]
        public Money Value
        {
            get
            {
                return Money.FromDatabase(RawValue);
            }

            set
            {
                RawValue = value.DatabaseAmount;
            }
        }

        /// <summary>
        /// The date the value was changed.
        /// </summary>
        [Required]
        public DateTime DateChanged {get; set;} = DateTime.UtcNow;
    }
}
