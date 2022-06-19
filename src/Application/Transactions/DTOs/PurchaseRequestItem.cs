namespace JahnDigital.StudentBank.Application.Transactions.DTOs;

/// <summary>
///     Represents an individual item in a larger purchase request.
/// </summary>
public class PurchaseRequestItem
{
    /// <summary>
    ///     Get or set the <ref cname="dal.Entities.Product" /> ID.
    /// </summary>
    public long ProductId { get; set; }

    /// <summary>
    ///     Get or set the quantity being purchased.
    /// </summary>
    public int Count { get; set; } = 1;
}
