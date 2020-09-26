using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using jahndigital.studentbank.server.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace jahndigital.studentbank.server.Entities
{
    /// <summary>
    /// Represents a stock.
    /// </summary>
    public class Stock
    {
        /// <summary>
        /// Unique ID of the stock.
        /// </summary>
        public long Id {get; set;}

        /// <summary>
        /// Gets the instance for this stock item.
        /// </summary>
        public Instance Instance {get; set;}

        /// <summary>
        /// Unique name of th
        /// </summary>
        /// <value></value>
        [MaxLength(10), Required]
        public string Symbol {get; set;}

        /// <summary>
        /// Name of the company.
        /// </summary>
        [MaxLength(32), Required]
        public string Name {get; set;}

        /// <summary>
        /// Total number of shares for the stock.
        /// </summary>
        [Required]
        public long TotalShares {get; set;} = 0;

        /// <summary>
        /// Total number of shares available to buy.
        /// </summary>
        [Required]
        public long AvailableShares {get; set;} = 0;

        /// <summary>
        /// The raw database value representing the current value of the share.
        /// Use CurrentValue to set this value.
        /// </summary>
        [Column("CurrentValue"), Required]
        public long RawCurrentValue { get; private set; } = 0;

        /// <summary>
        /// The current value of the stock.
        /// </summary>
        [NotMapped]
        public Money CurrentValue
        {
            get
            {
                return Money.FromDatabase(RawCurrentValue);
            }

            set
            {
                RawCurrentValue = value.DatabaseAmount;
            }
        }

        /// <summary>
        /// The history of the stock.
        /// </summary>
        public ICollection<StockHistory> History {get; set;}
    }
}
