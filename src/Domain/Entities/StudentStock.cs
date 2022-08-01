using JahnDigital.StudentBank.Domain.Common;
using JahnDigital.StudentBank.Domain.Exceptions;
using JahnDigital.StudentBank.Domain.ValueObjects;

namespace JahnDigital.StudentBank.Domain.Entities;

/// <summary>
///     Represents the Shares of a Stock owned by a specific student.
/// </summary>
public class StudentStock : AuditableEntity
{
    /// <summary>
    ///     The unique ID number of the entity.
    /// </summary>
    public long Id { get; private set; }

    /// <summary>
    ///     Get the student ID
    /// </summary>
    public long StudentId { get; private set; }

    /// <summary>
    ///     Get the ID number of the stock.
    /// </summary>
    public long StockId { get; private set; }

    /// <summary>
    ///     The student who owns the stock.
    /// </summary>
    public Student Student { get; private set; } = default!;

    /// <summary>
    ///     The stock the student owns.
    /// </summary>
    public Stock Stock { get; private set; } = default!;

    /// <summary>
    ///     The number of shares this student currently owns.
    /// </summary>
    public long SharesOwned { get; private set; }

    /// <summary>
    ///     Raw dollar amount of net contributions.
    /// </summary>
    public long RawNetContribution { get; private set; }

    /// <summary>
    ///     The dollar amount of net contributions.
    /// </summary>
    public Money NetContribution
    {
        get => Money.FromDatabase(RawNetContribution);
        private set => RawNetContribution = value.DatabaseAmount;
    }

    /// <summary>
    ///     Backing field for the history of a student's stocks.
    /// </summary>
    private ICollection<StudentStockHistory> _history = new HashSet<StudentStockHistory>();

    /// <summary>
    ///     The history of buy/sells for this stock.
    /// </summary>
    public IReadOnlyCollection<StudentStockHistory> History
    {
        get => _history.ToList();

        // @NOTE HotChocolate workaround
        private set => _history = value.ToList();
    }

    /// <summary>
    ///     Get or set the date that this stock was last purchased or sold.
    /// </summary>
    public DateTime DateLastActive { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Constructor for EF Core.
    /// </summary>
    private StudentStock() {}

    /// <summary>
    ///
    /// </summary>
    /// <param name="student"></param>
    /// <param name="stock"></param>
    public StudentStock(Student student, Stock stock)
    {
        Student = student;
        Stock = stock;
    }

    /// <summary>
    /// Calculate the amount returned or required to purchase the stock's quantity.
    /// </summary>
    /// <param name="quantity"></param>
    /// <returns></returns>
    public Money CalculatePurchaseAmount(int quantity)
    {
        if (quantity == 0)
        {
            return Money.Zero;
        }

        if (quantity < 0 && SharesOwned < -quantity)
        {
            throw new NotEnoughSharesException(Stock, this, quantity);
        }

        return Stock.CurrentValue * quantity * -1;
    }

    /// <summary>
    /// Purchase or sell shares and return the transaction amount.
    /// </summary>
    /// <param name="quantity"></param>
    /// <param name="transaction"></param>
    public void PurchaseShares(int quantity, Transaction transaction)
    {
        SharesOwned += quantity;
        DateLastActive = transaction.EffectiveDate;
        NetContribution += transaction.Amount * -1;
        _history.Add(new StudentStockHistory(this, transaction, quantity));
    }
}
