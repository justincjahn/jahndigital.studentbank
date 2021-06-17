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
    /// Describes the functionality of a given share type.
    /// </summary>
    public class ShareType
    {
        /// <summary>
        /// Unique id of the share type.
        /// </summary>
        public long Id {get; set;}

        /// <summary>
        /// Unique name for this share type.
        /// </summary>
        [MaxLength(32), Required]
        public string Name {get; set;} = default!;

        /// <summary>
        /// The raw interest/dividend rate from the database.  Use DividendRate
        /// to set this value.
        /// </summary>
        [Column("DividendRate"), Required]
        public long RawDividendRate {get; private set;}

        /// <summary>
        /// The interest/dividend rate for this share type.
        /// </summary>
        [NotMapped]
        public Rate DividendRate
        {
            get => Rate.FromDatabase(RawDividendRate);
            set => RawDividendRate = value.DatabaseValue;
        }

        /// <summary>
        /// The maximum number of withdrawals per period.
        /// </summary>
        [Required]
        public int WithdrawalLimitCount { get; set; } = 0;

        /// <summary>
        /// The period of time that must elapse prior to resetting the withdrawal limit.
        /// </summary>
        [Required]
        public Period WithdrawalLimitPeriod { get; set; } = Period.Monthly;

        /// <summary>
        /// Instead of denying the transaction, charge a fee.
        /// </summary>
        public bool WithdrawalLimitShouldFee { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        [Column("WithdrawalLimitFee"), Required]
        public long RawWithdrawalLimitFee {get; private set;} = 0;

        /// <summary>
        /// The fee charged when the withdrawal limit is exceeded.
        /// </summary>
        [NotMapped]
        public Money WithdrawalLimitFee {
            get => Money.FromDatabase(RawWithdrawalLimitFee);
            set => RawWithdrawalLimitFee = value.DatabaseAmount;
        }

        /// <summary>
        /// Get or set the collection of instances linked to this Share Type.
        /// </summary>
        /// <typeparam name="ShareTypeInstance"></typeparam>
        /// <returns></returns>
        public ICollection<ShareTypeInstance> ShareTypeInstances {get; set;} = new HashSet<ShareTypeInstance>();

        /// <summary>
        /// Gets the list of shares associated with this share type.
        /// </summary>
        public ICollection<Share> Shares {get; set;} = new HashSet<Share>();

        /// <summary>
        /// Get or set the date that the withdrawal limit was last reset.
        /// </summary>
        [DateTimeKind(DateTimeKind.Utc)]
        public DateTime WithdrawalLimitLastReset { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Get the date that the share type was created.
        /// </summary>
        [DateTimeKind(DateTimeKind.Utc)]
        public DateTime DateCreated {get; private set;} = DateTime.UtcNow;

        /// <summary>
        /// Get or set the date the share type was deleted.
        /// </summary>
        [DateTimeKind(DateTimeKind.Utc)]
        public DateTime? DateDeleted {get; set;} = null;
    }
}
