using JahnDigital.StudentBank.Domain.ValueObjects;

namespace JahnDigital.StudentBank.Domain.Entities;

/// <summary>
///     Represents a monetary transaction on a Share.
/// </summary>
public class Transaction
{
    /// <summary>
    ///     Unique ID number for the transaction.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     Get the ID number of the target share.
    /// </summary>
    public long TargetShareId { get; set; }

    /// <summary>
    ///     Type of transaction.
    /// </summary>
    public string TransactionType { get; set; } = default!;

    /// <summary>
    ///     The target share of the transaction.
    /// </summary>
    public Share TargetShare { get; set; } = default!;

    /// <summary>
    ///     An optional transaction comment.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    ///     Raw dollar amount of the transaction.
    /// </summary>
    public long RawAmount { get; private set; }

    /// <summary>
    ///     The dollar amount of the transaction.
    /// </summary>
    public Money Amount
    {
        get => Money.FromDatabase(RawAmount);
        set => RawAmount = value.DatabaseAmount;
    }

    /// <summary>
    ///     The raw new balance of the share.
    /// </summary>
    public long RawNewBalance { get; private set; }

    /// <summary>
    ///     The new balance of the share.
    /// </summary>
    public Money NewBalance
    {
        get => Money.FromDatabase(RawNewBalance);
        set => RawNewBalance = value.DatabaseAmount;
    }

    /// <summary>
    ///     The date the transaction was posted.
    /// </summary>
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
}
