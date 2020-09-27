using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace jahndigital.studentbank.server.Entities
{
    /// <summary>
    /// A group represents a collection of Students, usually as classes.
    /// </summary>
    public class Group
    {
        /// <summary>
        /// The unique ID number of the group.
        /// </summary>
        public long Id {get; set;}

        /// <summary>
        /// Name of the group.
        /// </summary>
        [MaxLength(32), Required]
        public string Name {get; set;}

        /// <summary>
        /// The instance of the group.
        /// </summary>
        [Required]
        public Instance Instance {get; set;}

        /// <summary>
        /// Gets the list of students associated with this group.
        /// </summary>
        public ICollection<Student> Students { get; set; } = new HashSet<Student>();
    }
}
