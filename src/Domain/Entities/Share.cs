using JahnDigital.StudentBank.Domain.ValueObjects;

namespace JahnDigital.StudentBank.Domain.Entities;

/// <summary>
///     Represents a deposit product with a balance.
/// </summary>
public class Share
{
    /// <summary>
    ///     The unique ID of the share.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     Get the ShareType ID
    /// </summary>
    public long ShareTypeId { get; set; }

    /// <summary>
    ///     Get the Student ID
    /// </summary>
    public long StudentId { get; set; }

    /// <summary>
    ///     The Share Type information for this product.
    /// </summary>
    public ShareType ShareType { get; set; } = default!;

    /// <summary>
    ///     The owner of the share.
    /// </summary>
    public Student Student { get; set; } = default!;

    /// <summary>
    /// </summary>
    public long RawBalance { get; private set; }

    /// <summary>
    ///     The current balance of the share.
    /// </summary>
    public Money Balance
    {
        get => Money.FromDatabase(RawBalance);
        set => RawBalance = value.DatabaseAmount;
    }

    /// <summary>
    ///     Backing field for DividendLastAmount
    /// </summary>
    public long RawDividendLastAmount { get; private set; }

    /// <summary>
    ///     Get or set the last dividend amount.
    /// </summary>
    public Money DividendLastAmount
    {
        get => Money.FromDatabase(RawDividendLastAmount);
        set => RawDividendLastAmount = value.DatabaseAmount;
    }

    /// <summary>
    ///     Backing field for TotalDividends.
    /// </summary>
    public long RawTotalDividends { get; private set; }

    /// <summary>
    ///     Get or set the total dividends paid to this account.
    /// </summary>
    public Money TotalDividends
    {
        get => Money.FromDatabase(RawTotalDividends);
        set => RawTotalDividends = value.DatabaseAmount;
    }

    /// <summary>
    ///     A list of transactions for the share.
    /// </summary>
    /// <typeparam name="Transaction"></typeparam>
    /// <returns></returns>
    public ICollection<Transaction> Transactions { get; set; } = new HashSet<Transaction>();

    /// <summary>
    ///     The number of limited withdrawals.
    /// </summary>
    public int LimitedWithdrawalCount { get; set; } = 0;

    /// <summary>
    ///     The last activity date of the share.
    /// </summary>
    public DateTime DateLastActive { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Get the date the share was created.
    /// </summary>
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Get or set the date the share was deleted.
    /// </summary>
    public DateTime? DateDeleted { get; set; } = null;
}
