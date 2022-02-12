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
    public long Id { get; set; }

    /// <summary>
    ///     Get the ID number of the StudentStock.
    /// </summary>
    public long StudentStockId { get; set; }

    /// <summary>
    ///     Get the transaction ID
    /// </summary>
    public long TransactionId { get; set; }

    /// <summary>
    ///     The stock the student currently owns.
    /// </summary>
    public StudentStock StudentStock { get; set; } = default!;

    /// <summary>
    ///     The monetary transaction that occurred as a result of the buy/sell.
    /// </summary>
    /// <value></value>
    public Transaction Transaction { get; set; } = default!;

    /// <summary>
    ///     The number of shares purchased or sold.
    /// </summary>
    public int Count { get; set; }

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
    public DateTime DatePosted { get; set; } = DateTime.UtcNow;
}
