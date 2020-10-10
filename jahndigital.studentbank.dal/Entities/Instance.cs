using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace jahndigital.studentbank.dal.Entities
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
        public List<Group> Groups { get; set; }

        /// <summary>
        /// Gets or sets the collection of stocks associated with this instance.
        /// </summary>
        public ICollection<Stock> Stocks { get; set; } = new HashSet<Stock>();

        /// <summary>
        /// Get or set a collection of share types linked to this instance.
        /// </summary>
        /// <typeparam name="ShareTypeInstance"></typeparam>
        /// <returns></returns>
        public ICollection<ShareTypeInstance> ShareTypeInstances { get; set; } = new HashSet<ShareTypeInstance>();

        /// <summary>
        /// Get the date the instance was created.
        /// </summary>
        public DateTime DateCreated {get; private set;} = DateTime.UtcNow;

        /// <summary>
        /// Get or set the date the instance was deleted.
        /// </summary>
        public DateTime? DateDeleted {get; set;} = null;
    }
}
