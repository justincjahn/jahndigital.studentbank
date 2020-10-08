namespace jahndigital.studentbank.server.Exceptions
{
    public class InvalidQuantityException : BaseException {
        /// <summary>
        /// Get the item of the product.
        /// </summary>
        public dal.Entities.Product Product {get; private set;}

        /// <summary>
        /// Get the quantity requested for purchase.
        /// </summary>
        public int QuantityRequested {get; private set;}

        public InvalidQuantityException(dal.Entities.Product product, int qtyNeeded) : base(
            $"Purchase failed because you requested {qtyNeeded} of {product.Name}, and only {product.Quantity} are available."
        ) {
            Product = product;
            QuantityRequested = qtyNeeded;
        }
    }
}
