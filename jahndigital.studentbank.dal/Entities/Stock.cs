using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.dal.Entities
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
        /// Get the instance ID
        /// </summary>
        [ForeignKey("Instance")]
        public long InstanceId {get; set;}

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
            get => Money.FromDatabase(RawCurrentValue);
            set => RawCurrentValue = value.DatabaseAmount;
        }

        /// <summary>
        /// Get the student stock.
        /// </summary>
        /// <typeparam name="StudentStock"></typeparam>
        /// <returns></returns>
        public ICollection<StudentStock> StudentStock {get; set;} = new HashSet<StudentStock>();

        /// <summary>
        /// The history of the stock.
        /// </summary>
        public ICollection<StockHistory> History {get; set;} = new HashSet<StockHistory>();

        /// <summary>
        /// Get the date that the stock was created.
        /// </summary>
        public DateTime DateCreated {get; private set;} = DateTime.UtcNow;

        /// <summary>
        /// Get or set the date that the stock was deleted.
        /// </summary>
        public DateTime? DateDeleted {get; set;} = null;
    }
}
