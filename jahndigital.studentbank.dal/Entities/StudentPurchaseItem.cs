using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.dal.Entities
{
    /// <summary>
    ///     Represents an individual item on a purchase.
    /// </summary>
    public class StudentPurchaseItem
    {
        /// <summary>
        ///     Get the unique key for this purchase item.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        ///     Get the ID of the StudentPurchase.
        /// </summary>
        [ForeignKey("StudentPurchase"), Required]
        public long StudentPurchaseId { get; set; }

        /// <summary>
        ///     Get the ID number of the purchased product.
        /// </summary>
        [ForeignKey("Product"), Required]
        public long ProductId { get; set; }

        /// <summary>
        ///     Get or set the StudentPurchase object that this purchase item belongs to.
        /// </summary>
        [Required]
        public StudentPurchase StudentPurchase { get; set; } = default!;

        /// <summary>
        ///     Get the product that was purchased.
        /// </summary>
        public Product Product { get; set; } = default!;

        /// <summary>
        ///     Get the total number of items purchased.
        /// </summary>
        public int Quantity { get; set; } = 1;

        /// <summary>
        ///     Get the raw (database stored) purchase cost of the item.
        /// </summary>
        [Column("PurchasePrice"), Required]
        public long RawPurchasePrice { get; private set; }

        /// <summary>
        ///     Get the purchase cost of the item.
        /// </summary>
        [NotMapped]
        public Money PurchasePrice
        {
            get => Money.FromDatabase(RawPurchasePrice);
            set => RawPurchasePrice = value.DatabaseAmount;
        }
    }
}