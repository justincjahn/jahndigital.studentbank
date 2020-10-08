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
        public long Id {get;set;}

        /// <summary>
        /// Get or set the name of the product.
        /// </summary>
        [MaxLength(128), Required]
        public string Name {get;set;}

        /// <summary>
        /// Get or set a short description of the product.
        /// </summary>
        [MaxLength(256)]
        public string Description {get; set;}

        /// <summary>
        /// Gets a list of images for the provided product.
        /// </summary>
        /// <typeparam name="ProductImages"></typeparam>
        /// <returns></returns>
        public ICollection<ProductImages> Images {get;set;} = new HashSet<ProductImages>();

        /// <summary>
        /// Get the raw (database storage format) cost.
        /// </summary>
        [Column("Cost"), Required]
        public long RawCost { get; private set; } = 0;

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
        public bool IsLimitedQuantity { get; set; } = false;

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
        /// Gets a list of <see cname="ProductGroup" /> objects linking this product to a group.
        /// </summary>
        /// <typeparam name="ProductGroup"></typeparam>
        /// <returns></returns>
        public ICollection<ProductGroup> ProductGroups {get; set;} = new HashSet<ProductGroup>();

        /// <summary>
        /// Get the date the product was created.
        /// </summary>
        public DateTime DateCreated {get; private set;} = DateTime.UtcNow;

        /// <summary>
        /// Get the date the product was deleted.
        /// </summary>
        public DateTime? DateDeleted {get; set;} = null;
    }
}
