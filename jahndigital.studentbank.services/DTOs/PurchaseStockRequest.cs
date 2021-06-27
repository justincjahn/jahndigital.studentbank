namespace jahndigital.studentbank.services.DTOs
{
    /// <summary>
    ///     Represents a request to purchase shares of a <see cref="dal.Entities.Stock" />.
    /// </summary>
    public class PurchaseStockRequest
    {
        /// <summary>
        ///     The <see cref="dal.Entities.Share" /> ID making the purchase.
        /// </summary>
        public long ShareId { get; set; } = default!;

        /// <summary>
        ///     The <see cref="dal.Entities.Stock" /> ID of the stock.
        /// </summary>
        public long StockId { get; set; } = default!;

        /// <summary>
        ///     The number of shares being purchased.
        /// </summary>
        public int Quantity { get; set; } = default!;
    }
}