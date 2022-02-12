using JahnDigital.StudentBank.Domain.Common;
using JahnDigital.StudentBank.Domain.ValueObjects;

namespace JahnDigital.StudentBank.Domain.Entities;

/// <summary>
///     Represents an individual item on a purchase.
/// </summary>
public class StudentPurchaseItem : EntityBase
{
    /// <summary>
    ///     The unique ID number of the entity.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     Get the ID of the StudentPurchase.
    /// </summary>
    public long StudentPurchaseId { get; set; }

    /// <summary>
    ///     Get the ID number of the purchased product.
    /// </summary>
    public long ProductId { get; set; }

    /// <summary>
    ///     Get or set the StudentPurchase object that this purchase item belongs to.
    /// </summary>
    public StudentPurchase StudentPurchase { get; set; } = default!;

    /// <summary>
    ///     Get the product that was purchased.
    /// </summary>
    public Product Product { get; set; } = default!;

    /// <summary>
    ///     Get the total number of items purchased.
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    ///     Get the raw (database stored) purchase cost of the item.
    /// </summary>
    public long RawPurchasePrice { get; private set; }

    /// <summary>
    ///     Get the purchase cost of the item.
    /// </summary>
    public Money PurchasePrice
    {
        get => Money.FromDatabase(RawPurchasePrice);
        set => RawPurchasePrice = value.DatabaseAmount;
    }
}
