using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace jahndigital.studentbank.dal.Entities
{
    /// <summary>
    ///     Table that joins one or more roles to one or more privileges.
    /// </summary>
    public class RolePrivilege
    {
        /// <summary>
        ///     Get or set the ID number of the role.
        /// </summary>
        [ForeignKey("Role"), Required]
        public long RoleId { get; set; }

        /// <summary>
        ///     Get or set the ID number of the privilege.
        /// </summary>
        [ForeignKey("Privilege"), Required]
        public long PrivilegeId { get; set; }

        /// <summary>
        ///     The Role associated with this privilege.
        /// </summary>
        [Required]
        public Role Role { get; set; } = default!;

        /// <summary>
        ///     The Privilege associated with this role.
        /// </summary>
        [Required]
        public Privilege Privilege { get; set; } = default!;
    }
}
