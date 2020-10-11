using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.dal.Entities
{
    /// <summary>
    /// A product offered to students for "purchase".
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Get the product ID
        /// </summary>
        public long Id {get; set;}

        /// <summary>
        /// Gets a list of images for the provided product.
        /// </summary>
        public ICollection<ProductImage> Images {get; set;} = new HashSet<ProductImage>();

        /// <summary>
        /// Get the raw (database storage format) cost.
        /// </summary>
        [Column("Cost"), Required]
        public long RawCost {get; private set;} = 0;

        /// <summary>
        /// Get the cost of the product.
        /// </summary>
        [NotMapped]
        public Money Cost {
            get => Money.FromDatabase(RawCost);
            set => RawCost = value.DatabaseAmount;
        }

        /// <summary>
        /// Gets or sets if the quantity of this product is limited.
        /// </summary>
        public bool IsLimitedQuantity {get; set;} = false;

        /// <summary>
        /// Backing field for <see cref="Quantity"/>.
        /// </summary>
        [Column("Quantity"), Required]
        private int _quantity = -1;

        /// <summary>
        /// Gets or sets the in-stock quantity of the product.
        /// </summary>
        [Required]
        public int Quantity {
            get => _quantity;
            set {
                if (IsLimitedQuantity) _quantity = value;
            }
        }

        /// <summary>
        /// Gets a list of <see cname="ProductInstance" /> objects linking this product to a group.
        /// </summary>
        public ICollection<ProductInstance> ProductInstances {get; set;} = new HashSet<ProductInstance>();

        /// <summary>
        /// Get the date the product was created.
        /// </summary>
        public DateTime DateCreated {get; private set;} = DateTime.UtcNow;

        /// <summary>
        /// Get the date the product was deleted.
        /// </summary>
        public DateTime? DateDeleted {get; set;} = null;

        /// <summary>
        /// Get or set the name of the product.
        /// </summary>
        [MaxLength(128), Required]
        public string Name {get; set;} = default!;

        /// <summary>
        /// Get or set a short description of the product.
        /// </summary>
        [MaxLength(256)]
        public string Description {get; set;} = default!;
    }
}
