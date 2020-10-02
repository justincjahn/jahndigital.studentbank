using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.dal.Entities
{
    /// <summary>
    /// Represents a purchase for students.  One purchase can have multiple line items.
    /// </summary>
    public class StudentPurchase
    {
        /// <summary>
        /// Gets the unique ID of the purchase.
        /// </summary>
        [Key]
        public long Id {get; set;}

        /// <summary>
        /// Get or set the ID number for the student.
        /// </summary>
        [ForeignKey("Student"), Required]
        public long StudentId {get; set;}

        /// <summary>
        /// Get or set the student associated with this purchase.
        /// </summary>
        [Required]
        public Student Student {get; set;}

        /// <summary>
        /// Get the raw (database stored) total cost of the purchase.
        /// </summary>
        [Column("TotalCost"), Required]
        public long RawTotalCost {get; private set;} = 0;

        /// <summary>
        /// Get or set the total cost of the purchase.
        /// </summary>
        [NotMapped]
        public Money TotalCost {
            get => Money.FromDatabase(RawTotalCost);
            set => RawTotalCost = value.DatabaseAmount;
        }

        /// <summary>
        /// Get a list of line items on this purchase.
        /// </summary>
        /// <typeparam name="StudentPurchaseItem"></typeparam>
        /// <returns></returns>
        public ICollection<StudentPurchaseItem> Items {get; set;} = new HashSet<StudentPurchaseItem>();
    }
}
