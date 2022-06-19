using MediatR;

namespace JahnDigital.StudentBank.Application.Transactions.DTOs;

/// <summary>
///     Data fields to make a purchase.
/// </summary>
public class PurchaseRequest : IRequest<long>
{
    /// <summary>
    ///     Get or set the ID of the share the purchase will be debited from.
    /// </summary>
    public long ShareId { get; set; }

    /// <summary>
    ///     A list of items being purchased.
    /// </summary>
    public IEnumerable<PurchaseRequestItem> Items { get; set; } = new HashSet<PurchaseRequestItem>();
}
