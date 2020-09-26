using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace jahndigital.studentbank.server.Entities
{
    /// <summary>
    /// Represents an instance of the software.  An instance is a collection of
    /// groups, students, transactions, and other data.
    /// </summary>
    public class Instance
    {
        /// <summary>
        /// The unique ID of the instance.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The description of the instance.
        /// </summary>
        [MaxLength(32), Required]
        public string Description { get; set; }

        /// <summary>
        /// If the instance is currently active for user login.
        /// </summary>
        [Required]
        public bool IsActive { get; set; } = false;

        /// <summary>
        /// Gets or sets the collection of groups associated with this instance.
        /// </summary>
        public ICollection<Group> Groups { get; set; }

        /// <summary>
        /// Gets or sets the collection of stocks associated with this instance.
        /// </summary>
        public ICollection<Stock> Stocks { get; set; }
    }
}
