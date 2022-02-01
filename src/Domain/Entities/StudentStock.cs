using JahnDigital.StudentBank.Domain.ValueObjects;

namespace JahnDigital.StudentBank.Domain.Entities;

/// <summary>
///     Represents the Shares of a Stock owned by a specific student.
/// </summary>
public class StudentStock
{
    /// <summary>
    ///     The unique ID of this entry.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     Get the student ID
    /// </summary>
    public long StudentId { get; set; }

    /// <summary>
    ///     Get the ID number of the stock.
    /// </summary>
    public long StockId { get; set; }

    /// <summary>
    ///     The student who owns the stock.
    /// </summary>
    public Student Student { get; set; } = default!;

    /// <summary>
    ///     The stock the student owns.
    /// </summary>
    public Stock Stock { get; set; } = default!;

    /// <summary>
    ///     The number of shares this student currently owns.
    /// </summary>
    public long SharesOwned { get; set; } = 0;

    /// <summary>
    ///     Raw dollar amount of net contributions.
    /// </summary>
    public long RawNetContribution { get; private set; } = 0;

    /// <summary>
    ///     The dollar amount of net contributions.
    /// </summary>
    public Money NetContribution
    {
        get => Money.FromDatabase(RawNetContribution);
        set => RawNetContribution = value.DatabaseAmount;
    }

    /// <summary>
    ///     The history of buy/sells for this stock.
    /// </summary>
    public ICollection<StudentStockHistory> History { get; set; } = new HashSet<StudentStockHistory>();

    /// <summary>
    ///     Get or set the date that this stock was last purchased or sold.
    /// </summary>
    public DateTime DateLastActive { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Get the date that the student originally acquired the stock.
    /// </summary>
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}
