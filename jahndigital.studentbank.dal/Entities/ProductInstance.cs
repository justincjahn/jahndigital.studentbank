using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace jahndigital.studentbank.dal.Entities
{
    /// <summary>
    ///     Links a <see cref="Product" /> to a specific <see cref="Instance" />.
    /// </summary>
    public class ProductInstance
    {
        /// <summary>
        ///     The ID number of the group.
        /// </summary>
        [ForeignKey("Instance"), Required]
        public long InstanceId { get; set; }

        /// <summary>
        ///     The ID number of the product.
        /// </summary>
        [ForeignKey("Product"), Required]
        public long ProductId { get; set; }

        /// <summary>
        ///     Gets or sets the <see cname="Instance" /> a product has been released to.
        /// </summary>
        [Required]
        public Instance Instance { get; set; } = default!;

        /// <summary>
        ///     The Product associated with this link.
        /// </summary>
        [Required]
        public Product Product { get; set; } = default!;
    }
}