using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace jahndigital.studentbank.dal.Entities
{
    /// <summary>
    /// Represents the Shares of a Stock owned by a specific student.
    /// </summary>
    public class StudentStock
    {
        /// <summary>
        /// The unique ID of this entry.
        /// </summary>
        public long Id {get; set;}

        /// <summary>
        /// Get the student ID
        /// </summary>
        [ForeignKey("Student")]
        public long StudentId {get;set;}

        /// <summary>
        /// The student who owns the stock.
        /// </summary>
        [Required]
        public Student Student {get; set;}

        /// <summary>
        /// Get the ID number of the stock.
        /// </summary>
        [ForeignKey("Stock")]
        public long StockId {get; set;}

        /// <summary>
        /// The stock the student owns.
        /// </summary>
        [Required]
        public Stock Stock {get; set;}

        /// <summary>
        /// The history of buy/sells for this stock.
        /// </summary>
        public ICollection<StudentStockHistory> History {get; set;} = new HashSet<StudentStockHistory>();

        /// <summary>
        /// Get or set the date that this stock was last purchased or sold.
        /// </summary>
        public DateTime DateLastActive {get; set;} = DateTime.UtcNow;

        /// <summary>
        /// Get the date that the student originally acquired the stock.
        /// </summary>
        public DateTime DateCreated {get; private set;} = DateTime.UtcNow;
    }
}
