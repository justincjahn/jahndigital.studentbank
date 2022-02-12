using JahnDigital.StudentBank.Domain.Common;
using JahnDigital.StudentBank.Domain.ValueObjects;

namespace JahnDigital.StudentBank.Domain.Entities;

/// <summary>
///     Represents the price history of a particular stock.
/// </summary>
public class StockHistory : EntityBase
{
    /// <summary>
    ///     The unique ID number of the entity.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     Get the stock ID
    /// </summary>
    public long StockId { get; set; }

    /// <summary>
    ///     The stock.
    /// </summary>
    public Stock Stock { get; set; } = default!;

    /// <summary>
    ///     Represents the raw value of the stock at the time.
    ///     Use Value to set this value.
    /// </summary>
    public long RawValue { get; private set; }

    /// <summary>
    ///     The new value of the stock.
    /// </summary>
    public Money Value
    {
        get => Money.FromDatabase(RawValue);
        set => RawValue = value.DatabaseAmount;
    }

    /// <summary>
    ///     The date the value was changed.
    /// </summary>
    public DateTime DateChanged { get; set; } = DateTime.UtcNow;
}
