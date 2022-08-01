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
    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    public long Id { get; private set; }

    /// <summary>
    /// Backing field for <see cref="Symbol"/>.
    /// </summary>
    private string _symbol = String.Empty;

    /// <summary>
    ///     The stock symbol.
    /// </summary>
    public string Symbol
    {
        get => _symbol;

        set
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"The stock symbol must not be empty.");
            }

            if (value.Length > 10)
            {
                throw new InvalidOperationException("The stock symbol cannot be larger than 10 characters.");
            }

            _symbol = value;
        }
    }

    /// <summary>
    ///     Name of the company.
    /// </summary>
    public string Name { get; set; } = String.Empty;

    /// <summary>
    /// The description of the stock.
    /// </summary>
    public string RawDescription { get; private set; } = String.Empty;

    /// <summary>
    /// The description of the stock, formatted for direct output in the UI.
    /// </summary>
    public string FormattedDescription { get; private set; } = String.Empty;

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
        private set => RawCurrentValue = value.DatabaseAmount;
    }

    /// <summary>
    ///     Get the student stock.
    /// </summary>
    public ICollection<StudentStock> StudentStock { get; set; } = new HashSet<StudentStock>();

    /// <summary>
    ///     Backing field for the history of a stock.
    /// </summary>
    private ICollection<StockHistory> _history = new HashSet<StockHistory>();

    /// <summary>
    ///     The history of the stock.  Read-only and can only be added to when the price of a stock changes.
    /// </summary>
    public IReadOnlyCollection<StockHistory> History
    {
        get => _history.ToList();

        // @NOTE HotChocolate workaround
        private set => _history = value.ToList();
    }

    /// <summary>
    ///     Get or set a collection of instances this stock is linked to.
    /// </summary>
    public ICollection<StockInstance> StockInstances { get; private set; } = new HashSet<StockInstance>();

    /// <summary>
    ///     Empty constructor for EF Core
    /// </summary>
    private Stock() {}

    /// <summary>
    ///     Initialize a new, valid stock record.
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="name"></param>
    /// <param name="initialValue"></param>
    /// <param name="rawDescription"></param>
    /// <param name="formattedDescription"></param>
    public Stock(string symbol, string name, Money initialValue, string rawDescription = "", string formattedDescription = "")
    {
        Symbol = symbol;
        Name = name;
        CurrentValue = initialValue;
        SetDescription(rawDescription, formattedDescription);
    }

    /// <summary>
    ///     Set the description of the stock.
    /// </summary>
    /// <param name="rawDescription"></param>
    /// <param name="formattedDescription"></param>
    public void SetDescription(string rawDescription, string formattedDescription)
    {
        RawDescription = rawDescription;
        FormattedDescription = formattedDescription;
    }

    /// <summary>
    /// Sets the current value of the stock and adds a history entry.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="effectiveDate"></param>
    public void SetValue(Money value, DateTime? effectiveDate = null)
    {
        var history = new StockHistory()
        {
            Stock = this,
            Value = value,
            DateChanged = effectiveDate ?? DateTime.UtcNow
        };

        _history.Add(history);
        CurrentValue = value;
    }
}
