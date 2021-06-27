using jahndigital.studentbank.dal.Entities;

namespace jahndigital.studentbank.services.Exceptions
{
    public class InvalidQuantityException : BaseException
    {
        public InvalidQuantityException(Product product, int qtyNeeded) : base(
            $"Purchase failed because you requested {qtyNeeded} of {product.Name}, and only {product.Quantity} are available."
        )
        {
            Product = product;
            QuantityRequested = qtyNeeded;
        }

        /// <summary>
        ///     Get the item of the product.
        /// </summary>
        public Product Product { get; }

        /// <summary>
        ///     Get the quantity requested for purchase.
        /// </summary>
        public int QuantityRequested { get; }
    }
}