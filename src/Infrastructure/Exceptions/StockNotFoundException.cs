namespace JahnDigital.StudentBank.Infrastructure.Exceptions;

public class StockNotFoundException : DatabaseException
{
    public StockNotFoundException(long stockId) : base(
        $"Stock #{stockId} not found."
    )
    {
        StockId = stockId;
    }

    public long StockId { get; }
}

