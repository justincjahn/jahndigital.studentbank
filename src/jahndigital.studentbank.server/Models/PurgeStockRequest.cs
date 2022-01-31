using System;

namespace jahndigital.studentbank.server.Models
{
    /// <summary>
    /// A request to purge the history of a given stock.
    /// </summary>
    public class PurgeStockRequest
    {
        /// <summary>
        ///     The stock for which to purge history.
        /// </summary>
        public long StockId { get; set; } = default!;

        /// <summary>
        ///     A cutoff date.  Stock history entries older than this date will be purged.
        /// </summary>
        public DateTime Date { get; set; } = DateTime.Now.AddDays(-90);
    }
}
