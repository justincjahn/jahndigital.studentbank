using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.ValueObjects;
using MediatR;

namespace JahnDigital.StudentBank.Application.Transactions.DTOs;

public class TransferRequest : IRequest<(Transaction, Transaction)>
{
    /// <summary>
    /// The source <see cref="JahnDigital.StudentBank.Domain.Entities.Share"/> ID.
    /// </summary>
    public long SourceShareId { get; init; }

    /// <summary>
    /// The destination <see cref="JahnDigital.StudentBank.Domain.Entities.Share"/> ID.
    /// </summary>
    public long DestinationShareId { get; init; }

    /// <summary>
    /// The dollar amount to transfer.
    /// </summary>
    public Money Amount { get; init; } = Money.Zero;

    /// <summary>
    /// An optional transaction comment.
    /// </summary>
    public string? Comment { get; init; } = null;

    /// <summary>
    /// The optional effective date of the transactions.
    /// </summary>
    public DateTime? EffectiveDate { get; init; } = null;

    /// <summary>
    /// If the transfer is allowed to take the source share negative.
    /// </summary>
    public bool TakeNegative { get; init; } = false;

    /// <summary>
    /// If the withdrawal limit should be assessed on the source share.
    /// </summary>
    public bool WithdrawalLimit { get; init; } = true;
}
