using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace jahndigital.studentbank.server.Entities
{
    /// <summary>
    /// Represents a security event/privilege in the system.
    /// </summary>
    public class Privilege
    {
        /// <summary>
        /// Unique ID of the privilege.
        /// </summary>
        public long Id {get; set;}

        /// <summary>
        /// Unique name of the privilege.
        /// </summary>
        [MaxLength(16), Required]
        public string Name {get; set;}

        /// <summary>
        /// A short description of the privilege.
        /// </summary>
        [MaxLength(128), Required]
        public string Description {get; set;}

        /// <summary>
        /// Gets the intermediate table that links a Privilege to a collection or roles.
        /// </summary>
        public ICollection<RolePrivilege> RolePrivileges { get; set; } = new HashSet<RolePrivilege>();
    }
}
