using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.ValueObjects;
using MediatR;

namespace JahnDigital.StudentBank.Application.Transactions.DTOs;

public record TransactionRequest : IRequest<Transaction>
{
    /// <summary>
    /// The Share ID to post the transaction to.
    /// </summary>
    public long ShareId { get; init; }

    /// <summary>
    /// The amount to post.
    /// </summary>
    public Money Amount { get; init; } = default!;

    /// <summary>
    /// An optional transaction comment.
    /// </summary>
    public string? Comment { get; init; }

    /// <summary>
    /// An optional transaction type.
    /// </summary>
    public string? Type { get; init; } = null;

    /// <summary>
    /// An optional Effective Date for the transaction.
    /// </summary>
    public DateTime? EffectiveDate { get; init; } = null;

    /// <summary>
    /// If the transaction is allowed to take the share negative.
    /// </summary>
    public bool? TakeNegative { get; init; } = false;

    /// <summary>
    /// If withdrawal limits should be assessed for this transaction.
    /// </summary>
    public bool? WithdrawalLimit { get; init; } = true;
}
