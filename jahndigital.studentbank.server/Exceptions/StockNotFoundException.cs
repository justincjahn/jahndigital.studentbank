namespace jahndigital.studentbank.server.Exceptions
{
    public class StockNotFoundException : BaseException
    {
        public long StockId { get; private set; }

        public StockNotFoundException(long stockId) : base (
            $"Stock #{stockId} not found."
        ) {
            StockId = stockId;
        }
    }
}
