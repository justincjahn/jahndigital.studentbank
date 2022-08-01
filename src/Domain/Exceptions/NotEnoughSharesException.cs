using JahnDigital.StudentBank.Domain.Entities;

namespace JahnDigital.StudentBank.Domain.Exceptions;

/// <summary>
/// An exception thrown when the student tries to sell shares of a stock in excess of what they currently own.
/// </summary>
public class NotEnoughSharesException : BaseException
{
    /// <summary>
    /// Get the number of shares requested to be sold.
    /// </summary>
    public int Shares { get; init; }

    /// <summary>
    /// Get the stock that triggered the exception.
    /// </summary>
    public Stock Stock { get; init; }

    /// <summary>
    /// Get the StudentStock that triggered the exception.
    /// </summary>
    public StudentStock StudentStock { get; init; }

    /// <summary>
    ///
    /// </summary>
    /// <param name="stock"></param>
    /// <param name="studentStock"></param>
    /// <param name="quantityRequested"></param>
    public NotEnoughSharesException(Stock stock, StudentStock studentStock, int quantityRequested) : base(
        $"Purchase failed because you requested to sell {quantityRequested} of {stock.Symbol}, and you only own {studentStock.SharesOwned} shares."
    )
    {
        Shares = quantityRequested;
        Stock = stock;
        StudentStock = studentStock;
    }
}
