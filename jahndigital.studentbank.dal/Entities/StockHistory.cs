using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using jahndigital.studentbank.dal.Attributes;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.dal.Entities
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
        /// Get the stock ID
        /// </summary>
        [ForeignKey("Stock")]
        public long StockId {get; set;}

        /// <summary>
        /// The stock.
        /// </summary>
        [Required]
        public Stock Stock {get; set;} = default!;

        /// <summary>
        /// Represents the raw value of the stock at the time.
        /// Use Value to set this value.
        /// </summary>
        [Column("Value"), Required]
        public long RawValue {get; private set;} = 0;

        /// <summary>
        /// The new value of the stock.
        /// </summary>
        [NotMapped]
        public Money Value
        {
            get => Money.FromDatabase(RawValue);
            set => RawValue = value.DatabaseAmount;
        }

        /// <summary>
        /// The date the value was changed.
        /// </summary>
        [Required, DateTimeKind(DateTimeKind.Utc)]
        public DateTime DateChanged {get; set;} = DateTime.UtcNow;
    }
}
