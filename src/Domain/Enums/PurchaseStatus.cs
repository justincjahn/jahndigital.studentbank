using JahnDigital.StudentBank.Domain.Entities;

namespace JahnDigital.StudentBank.Domain.Enums
{
    /// <summary>
    ///     The purchase status of a <see cref="StudentPurchase" />.
    /// </summary>
    public enum PurchaseStatus
    {
        /// The order has been placed
        Placed,

        /// Order fulfillment is in-progress
        InProgress,

        /// The order is complete
        Complete,

        /// The order has been cancelled
        Cancelled
    }
}
