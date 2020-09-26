using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace jahndigital.studentbank.server.Entities
{
    /// <summary>
    /// Represents a role assigned to a User.  Roles are a collection of privileges
    /// that enable access to an API call.
    /// </summary>
    public class Role
    {
        /// <summary>
        /// The unique ID of the role.
        /// </summary>
        public long Id {get; set;}

        /// <summary>
        /// The name of the role.
        /// </summary>
        [MaxLength(32), Required]
        public string Name {get; set;}

        /// <summary>
        /// A list of Privileges assigned to this role.
        /// </summary>
        public ICollection<RolePrivilege> RolePrivileges {get; set;}
    }
}
