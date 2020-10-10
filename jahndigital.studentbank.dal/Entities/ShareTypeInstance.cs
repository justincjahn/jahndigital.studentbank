using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace jahndigital.studentbank.dal.Entities
{
    /// <summary>
    /// Table that joins one or more <see cref="ShareType"/>s to one or more <see cref="Instance"/>s.
    /// </summary>
    public class ShareTypeInstance
    {
        /// <summary>
        /// The ID number of the <see cref="ShareType"/>
        /// </summary>
        [ForeignKey("ShareType"), Required]
        public long ShareTypeId { get; set; }

        /// <summary>
        /// The <see cref="ShareType"/> assigned to the instance.
        /// </summary>
        [Required]
        public ShareType ShareType { get; set; }

        /// <summary>
        /// The <see cref="Instance"/> ID.
        /// </summary>
        [ForeignKey("Instance"), Required]
        public long InstanceId { get; set; }

        /// <summary>
        /// The <see cref="Instance"/> the Share Type is linked to.
        /// </summary>
        [Required]
        public Instance Instance { get; set; }
    }
}
