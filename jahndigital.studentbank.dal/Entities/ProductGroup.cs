using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace jahndigital.studentbank.dal.Entities
{
    /// <summary>
    /// Table that joins one or more products to one or more groups.
    /// </summary>
    public class ProductGroup
    {
        /// <summary>
        /// The ID number of the group.
        /// </summary>
        [ForeignKey("Group")]
        public long GroupId { get; set; }

        /// <summary>
        /// Gets or sets the <see cname="Group" /> a product has been released to.
        /// </summary>
        [Required]
        public Group Group {get; set;}

        /// <summary>
        /// The ID number of the product.
        /// </summary>
        [ForeignKey("Product")]
        public long ProductId { get; set; }

        /// <summary>
        /// The Product associated with this link.
        /// </summary>
        [Required]
        public Product Product {get; set;}
    }
}
