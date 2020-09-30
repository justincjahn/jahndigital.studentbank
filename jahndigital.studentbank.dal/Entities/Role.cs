using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace jahndigital.studentbank.dal.Entities
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
        /// Short description of the role.
        /// </summary>
        [MaxLength(128)]
        public string Description {get; set;} = String.Empty;

        /// <summary>
        /// If the role is a built-in role that cannot be deleted.
        /// </summary>
        public bool IsBuiltIn {get; set;} = false;

        /// <summary>
        /// A list of Privileges assigned to this role.
        /// </summary>
        public ICollection<RolePrivilege> RolePrivileges {get; set;} = new HashSet<RolePrivilege>();
    }
}
