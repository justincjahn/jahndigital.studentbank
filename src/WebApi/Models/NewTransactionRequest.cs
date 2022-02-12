using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.ValueObjects;

namespace JahnDigital.StudentBank.WebApi.Models;

/// <summary>
///     Represents a request to post a monetary transaction.
/// </summary>
public class NewTransactionRequest
{
    /// <summary>
    ///     Get or set the <see cref="Share" /> ID to post to.
    /// </summary>
    public long ShareId { get; set; } = default!;

    /// <summary>
    ///     Get or set the amount to post.
    /// </summary>
    public Money Amount { get; set; } = default!;

    /// <summary>
    ///     Get or set an optional comment for the transaction.
    /// </summary>
    public string? Comment { get; set; } = default!;

    /// <summary>
    ///     Allow the transaction to take the account negative.
    /// </summary>
    public bool? TakeNegative { get; set; } = false;
}
