using JahnDigital.StudentBank.Domain.Common;
using JahnDigital.StudentBank.Domain.ValueObjects;

namespace JahnDigital.StudentBank.Domain.Entities;

/// <summary>
///     Represents stock buy/sells over time for a specific Student.
/// </summary>
public class StudentStockHistory : EntityBase
{
    /// <summary>
    ///     The unique ID of this transaction.
    /// </summary>
    public long Id { get; private set; }

    /// <summary>
    ///     Get the ID number of the StudentStock.
    /// </summary>
    public long StudentStockId { get; private set; }

    /// <summary>
    ///     Get the transaction ID
    /// </summary>
    public long TransactionId { get; private set; }

    /// <summary>
    ///     The stock the student currently owns.
    /// </summary>
    public StudentStock StudentStock { get; private set; } = default!;

    /// <summary>
    ///     The monetary transaction that occurred as a result of the buy/sell.
    /// </summary>
    /// <value></value>
    public Transaction Transaction { get; private set; } = default!;

    /// <summary>
    ///     The number of shares purchased or sold.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    ///     Raw amount of each share at the time of purchase.
    /// </summary>
    public long RawAmount { get; private set; }

    /// <summary>
    ///     The amount of each share at the time of purchase.
    /// </summary>
    public Money Amount
    {
        get => Money.FromDatabase(RawAmount);
        set => RawAmount = value.DatabaseAmount;
    }

    /// <summary>
    ///     The date the stock was bought/sold.
    /// </summary>
    public DateTime DatePosted { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Constructor for EF Core.
    /// </summary>
    public StudentStockHistory() {}

    /// <summary>
    /// Initialize a new, valid record.
    /// </summary>
    /// <param name="studentStock"></param>
    /// <param name="transaction"></param>
    /// <param name="quantity"></param>
    public StudentStockHistory(StudentStock studentStock, Transaction transaction, int quantity)
    {
        StudentStock = studentStock;
        Transaction = transaction;
        DatePosted = transaction.EffectiveDate;
        Amount = StudentStock.Stock.CurrentValue;
        Count = quantity;
    }
}
