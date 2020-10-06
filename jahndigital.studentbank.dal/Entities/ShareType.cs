using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.dal.Entities
{
    /// <summary>
    /// Describes the functionality of a given share type.
    /// </summary>
    public class ShareType
    {
        /// <summary>
        /// Unique id of the share type.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Unique name for this share type.
        /// </summary>
        [MaxLength(32), Required]
        public string Name { get; set; }

        /// <summary>
        /// The raw interest/dividend rate from the database.  Use DividendRate
        /// to set this value.
        /// </summary>
        [Column("DividendRate"), Required]
        public long RawDividendRate { get; private set; }

        /// <summary>
        /// The interest/dividend rate for this share type.
        /// </summary>
        [NotMapped]
        public Rate DividendRate
        {
            get => Rate.FromDatabase(RawDividendRate);
            set => RawDividendRate = value.DatabaseValue;
        }

        /// <summary>
        /// Gets the list of shares associated with this share type.
        /// </summary>
        public ICollection<Share> Shares { get; set; } = new HashSet<Share>();

        /// <summary>
        /// Get the date that the share type was created.
        /// </summary>
        public DateTime DateCreated {get; private set;} = DateTime.UtcNow;

        /// <summary>
        /// Get or set the date the share type was deleted.
        /// </summary>
        public DateTime? DateDeleted {get; set;} = null;
    }
}
