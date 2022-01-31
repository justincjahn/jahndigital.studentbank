using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using jahndigital.studentbank.dal.Attributes;
using jahndigital.studentbank.dal.Enums;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.dal.Entities
{
    /// <summary>
    ///     Represents a purchase for students.  One purchase can have multiple line items.
    /// </summary>
    public class StudentPurchase
    {
        /// <summary>
        ///     Gets the unique ID of the purchase.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        ///     Get or set the ID number for the student.
        /// </summary>
        [ForeignKey("Student"), Required]
        public long StudentId { get; set; }

        /// <summary>
        ///     Get or set the student associated with this purchase.
        /// </summary>
        [Required]
        public Student Student { get; set; } = default!;

        /// <summary>
        ///     Get the raw (database stored) total cost of the purchase.
        /// </summary>
        [Column("TotalCost"), Required]
        public long RawTotalCost { get; private set; }

        /// <summary>
        ///     Get or set the total cost of the purchase.
        /// </summary>
        [NotMapped]
        public Money TotalCost
        {
            get => Money.FromDatabase(RawTotalCost);
            set => RawTotalCost = value.DatabaseAmount;
        }

        /// <summary>
        ///     Get or set the status of the purchase.
        /// </summary>
        [Column(TypeName = "nvarchar(32)"), Required]
        public PurchaseStatus Status { get; set; } = PurchaseStatus.Placed;

        /// <summary>
        ///     Get a list of line items on this purchase.
        /// </summary>
        public ICollection<StudentPurchaseItem> Items { get; set; } = new HashSet<StudentPurchaseItem>();

        /// <summary>
        ///     Get the date that the purchase was created.
        /// </summary>
        [DateTimeKind]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }
}
