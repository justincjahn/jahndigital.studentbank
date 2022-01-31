using jahndigital.studentbank.dal.Entities;

namespace jahndigital.studentbank.services.Exceptions
{
    public class InvalidShareQuantityException : BaseException
    {
        public InvalidShareQuantityException(Stock stock, long qtyNeeded) : base(
            $"Purchase failed because you requested {qtyNeeded} of {stock.Symbol}, and only {stock.AvailableShares} are available."
        )
        {
            Stock = stock;
            QuantityRequested = qtyNeeded;
        }

        /// <summary>
        ///     Get the item of the product.
        /// </summary>
        public Stock Stock { get; }

        /// <summary>
        ///     Get the quantity requested for purchase.
        /// </summary>
        public long QuantityRequested { get; }
    }
}
