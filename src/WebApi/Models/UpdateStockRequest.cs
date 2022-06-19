using JahnDigital.StudentBank.Domain.ValueObjects;

namespace JahnDigital.StudentBank.WebApi.Models
{
    public class UpdateStockRequest
    {
        /// <summary>
        ///     The ID number of the stock
        /// </summary>
        public long Id { get; set; } = default!;

        /// <summary>
        ///     Unique symbol of the stock.
        /// </summary>
        public string? Symbol { get; set; } = null;

        /// <summary>
        ///     Name of the company
        /// </summary>
        public string? Name { get; set; } = null;

        /// <summary>
        ///     Total number of shares for the stock.
        /// </summary>
        public long? TotalShares { get; set; } = null;

        /// <summary>
        ///     The current value of the stock.
        /// </summary>
        public Money? CurrentValue { get; set; } = null;
    }
}
