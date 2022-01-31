using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using jahndigital.studentbank.dal.Attributes;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.dal.Entities
{
    /// <summary>
    ///     Represents a deposit product with a balance.
    /// </summary>
    public class Share
    {
        /// <summary>
        ///     The unique ID of the share.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Get the ShareType ID
        /// </summary>
        [ForeignKey("ShareType")]
        public long ShareTypeId { get; set; }

        /// <summary>
        ///     Get the Student ID
        /// </summary>
        [ForeignKey("Student")]
        public long StudentId { get; set; }

        /// <summary>
        ///     The Share Type information for this product.
        /// </summary>
        [Required]
        public ShareType ShareType { get; set; } = default!;

        /// <summary>
        ///     The owner of the share.
        /// </summary>
        [Required]
        public Student Student { get; set; } = default!;

        /// <summary>
        /// </summary>
        [Column("Balance"), Required]
        public long RawBalance { get; private set; }

        /// <summary>
        ///     The current balance of the share.
        /// </summary>
        [NotMapped]
        public Money Balance
        {
            get => Money.FromDatabase(RawBalance);
            set => RawBalance = value.DatabaseAmount;
        }

        /// <summary>
        ///     Backing field for DividendLastAmount
        /// </summary>
        [Column("DividendLastAmount"), Required]
        public long RawDividendLastAmount { get; private set; }

        /// <summary>
        ///     Get or set the last dividend amount.
        /// </summary>
        [NotMapped]
        public Money DividendLastAmount
        {
            get => Money.FromDatabase(RawDividendLastAmount);
            set => RawDividendLastAmount = value.DatabaseAmount;
        }

        /// <summary>
        ///     Backing field for TotalDividends.
        /// </summary>
        [Column("TotalDividends"), Required]
        public long RawTotalDividends { get; private set; }

        /// <summary>
        ///     Get or set the total dividends paid to this account.
        /// </summary>
        [NotMapped]
        public Money TotalDividends
        {
            get => Money.FromDatabase(RawTotalDividends);
            set => RawTotalDividends = value.DatabaseAmount;
        }

        /// <summary>
        ///     A list of transactions for the share.
        /// </summary>
        /// <typeparam name="Transaction"></typeparam>
        /// <returns></returns>
        public ICollection<Transaction> Transactions { get; set; } = new HashSet<Transaction>();

        /// <summary>
        ///     The number of limited withdrawals.
        /// </summary>
        [Required]
        public int LimitedWithdrawalCount { get; set; } = 0;

        /// <summary>
        ///     The last activity date of the share.
        /// </summary>
        [Required, DateTimeKind]
        public DateTime DateLastActive { get; set; } = DateTime.UtcNow;

        /// <summary>
        ///     Get the date the share was created.
        /// </summary>
        [DateTimeKind]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        /// <summary>
        ///     Get or set the date the share was deleted.
        /// </summary>
        [DateTimeKind]
        public DateTime? DateDeleted { get; set; } = null;
    }
}
