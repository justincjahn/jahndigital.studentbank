using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.server.Models
{
    public class NewStockRequest
    {
        /// <summary>
        ///     Unique symbol of the stock.
        /// </summary>
        public string Symbol { get; set; } = default!;

        /// <summary>
        ///     Name of the company
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        ///     Total number of shares for the stock.
        /// </summary>
        public long TotalShares { get; set; } = 0;

        /// <summary>
        ///     The current value of the stock.
        /// </summary>
        public Money CurrentValue { get; set; } = default!;
    }
}