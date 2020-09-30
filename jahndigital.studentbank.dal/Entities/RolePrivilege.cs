using System;
using System.ComponentModel.DataAnnotations;

namespace jahndigital.studentbank.dal.Entities
{
    /// <summary>
    /// Table that joins one or more roles to one or more privileges.
    /// </summary>
    public class RolePrivilege
    {
        /// <summary>
        /// The unique ID of this row.
        /// </summary>
        [Key]
        public long Id {get; set;}

        /// <summary>
        /// 
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// The Role associated with this privilege.
        /// </summary>
        [Required]
        public Role Role {get; set;}


        /// <summary>
        /// 
        /// </summary>
        public long PrivilegeId { get; set; }

        /// <summary>
        /// The Privilege associated with this role.
        /// </summary>
        [Required]
        public Privilege Privilege {get; set;}
    }
}
