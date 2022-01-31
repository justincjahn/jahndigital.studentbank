using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using jahndigital.studentbank.dal.Attributes;

namespace jahndigital.studentbank.dal.Entities
{
    /// <summary>
    ///     Represents a security event/privilege in the system.
    /// </summary>
    public class Privilege
    {
        /// <summary>
        ///     Unique ID of the privilege.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Unique name of the privilege.
        /// </summary>
        [MaxLength(64), Required]
        public string Name { get; set; } = default!;

        /// <summary>
        ///     A short description of the privilege.
        /// </summary>
        [MaxLength(128), Required]
        public string Description { get; set; } = default!;

        /// <summary>
        ///     Gets the intermediate table that links a Privilege to a collection or roles.
        /// </summary>
        public ICollection<RolePrivilege> RolePrivileges { get; set; } = new HashSet<RolePrivilege>();

        /// <summary>
        ///     Get the date that the privilege was created.
        /// </summary>
        [DateTimeKind]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }
}
