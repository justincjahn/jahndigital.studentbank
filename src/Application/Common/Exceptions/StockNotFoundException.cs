namespace JahnDigital.StudentBank.Application.Common.Exceptions;

public class StockNotFoundException : NotFoundException
{
    public StockNotFoundException(long stockId) : base(
        $"Stock #{stockId} not found."
    )
    {
        StockId = stockId;
    }

    public long StockId { get; }
}
