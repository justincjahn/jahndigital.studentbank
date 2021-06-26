namespace jahndigital.studentbank.services.Exceptions
{
    public class InvalidShareQuantityException : BaseException
    {
        /// <summary>
        /// Get the item of the product.
        /// </summary>
        public dal.Entities.Stock Stock {get; private set;}

        /// <summary>
        /// Get the quantity requested for purchase.
        /// </summary>
        public long QuantityRequested {get; private set;}

        public InvalidShareQuantityException(dal.Entities.Stock stock, long qtyNeeded) : base(
            $"Purchase failed because you requested {qtyNeeded} of {stock.Symbol}, and only {stock.AvailableShares} are available."
        ) {
            Stock = stock;
            QuantityRequested = qtyNeeded;
        }
    }
}
