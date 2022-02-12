using JahnDigital.StudentBank.Domain.Common;
using JahnDigital.StudentBank.Domain.ValueObjects;

namespace JahnDigital.StudentBank.Domain.Entities;

/// <summary>
///     Represents a stock.
/// </summary>
public class Stock : SoftDeletableEntity
{
    /// <summary>
    ///     The unique ID number of the entity.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     Unique name of th
    /// </summary>
    /// <value></value>
    public string Symbol { get; set; } = default!;

    /// <summary>
    ///     Name of the company.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    ///     Total number of shares for the stock.
    /// </summary>
    public long TotalShares { get; set; } = 10000000;

    /// <summary>
    ///     Total number of shares available to buy.
    /// </summary>
    public long AvailableShares { get; set; }

    /// <summary>
    ///     The raw database value representing the current value of the share.
    ///     Use CurrentValue to set this value.
    /// </summary>
    public long RawCurrentValue { get; private set; }

    /// <summary>
    ///     The current value of the stock.
    /// </summary>
    public Money CurrentValue
    {
        get => Money.FromDatabase(RawCurrentValue);
        set => RawCurrentValue = value.DatabaseAmount;
    }

    /// <summary>
    ///     Get the student stock.
    /// </summary>
    public ICollection<StudentStock> StudentStock { get; set; } = new HashSet<StudentStock>();

    /// <summary>
    ///     The history of the stock.
    /// </summary>
    public ICollection<StockHistory> History { get; set; } = new HashSet<StockHistory>();

    /// <summary>
    ///     Get or set a collection of instances this stock is linked to.
    /// </summary>
    public ICollection<StockInstance> StockInstances { get; set; } = new HashSet<StockInstance>();
}
