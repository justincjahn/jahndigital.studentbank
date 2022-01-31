using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using jahndigital.studentbank.dal.Attributes;

namespace jahndigital.studentbank.dal.Entities
{
    /// <summary>
    ///     A group represents a collection of Students, usually as classes.
    /// </summary>
    public class Group
    {
        /// <summary>
        ///     The unique ID number of the group.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Get the Instance ID.
        /// </summary>
        [ForeignKey("Instance")]
        public long InstanceId { get; set; }

        /// <summary>
        ///     Name of the group.
        /// </summary>
        [MaxLength(32), Required]
        public string Name { get; set; } = default!;

        /// <summary>
        ///     The instance of the group.
        /// </summary>
        [Required]
        public Instance Instance { get; set; } = default!;

        /// <summary>
        ///     Gets the list of students associated with this group.
        /// </summary>
        public ICollection<Student> Students { get; set; } = new HashSet<Student>();

        /// <summary>
        ///     Get the date the group was created.
        /// </summary>
        [DateTimeKind]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        /// <summary>
        ///     Get or set the date the group was deleted.
        /// </summary>
        [DateTimeKind]
        public DateTime? DateDeleted { get; set; } = null;
    }
}
