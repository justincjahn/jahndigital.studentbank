namespace jahndigital.studentbank.services.Exceptions
{
    public class StockNotFoundException : BaseException
    {
        public StockNotFoundException(long stockId) : base(
            $"Stock #{stockId} not found."
        )
        {
            StockId = stockId;
        }

        public long StockId { get; }
    }
}