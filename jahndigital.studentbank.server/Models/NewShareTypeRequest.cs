using jahndigital.studentbank.dal.Enums;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.server.Models
{
    /// <summary>
    /// Request data to create a Share Type.
    /// </summary>
    public class NewShareTypeRequest
    {
        /// <summary>
        /// Get or set the name of the share type.
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// Get or set the dividend rate.
        /// </summary>
        public Rate DividendRate {get; set;} = default!;

        /// <summary>
        /// Get or set the number of withdrawals allowed per period.  Use zero to disable.
        /// </summary>
        public int? WithdrawalLimitCount { get; set; } = default!;

        /// <summary>
        /// Get or set the withdrawal limit period to use when resetting the withdrawal limit counters.
        /// </summary>
        public Period? WithdrawalLimitPeriod { get; set; } = default!;

        /// <summary>
        /// Get or set if withdrawals over the <see cref="WithdrawalLimitCount"/> should fee instead of being declined.
        /// </summary>
        public bool? WithdrawalLimitShouldFee { get; set; } = default!;

        /// <summary>
        /// Get or set the amount to fee if <see cref="WithdrawalLimitShouldFee"/> is true.
        /// </summary>
        public Money? WithdrawalLimitFee { get; set; } = default!;
    }
}
