using JahnDigital.StudentBank.Domain.Common;
using JahnDigital.StudentBank.Domain.ValueObjects;

namespace JahnDigital.StudentBank.Domain.Entities;

/// <summary>
///     A product offered to students for "purchase".
/// </summary>
public class Product : SoftDeletableEntity
{
    /// <summary>
    ///     The unique ID number of the entity.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     Gets a list of images for the provided product.
    /// </summary>
    public ICollection<ProductImage> Images { get; set; } = new HashSet<ProductImage>();

    /// <summary>
    ///     Get the raw (database storage format) cost.
    /// </summary>
    public long RawCost { get; private set; }

    /// <summary>
    ///     Get the cost of the product.
    /// </summary>
    public Money Cost
    {
        get => Money.FromDatabase(RawCost);
        set => RawCost = value.DatabaseAmount;
    }

    /// <summary>
    ///     Gets or sets if the quantity of this product is limited.
    /// </summary>
    public bool IsLimitedQuantity { get; set; }

    /// <summary>
    ///     Gets or sets the in-stock quantity of the product.
    /// </summary>
    public int Quantity { get; set; } = -1;

    /// <summary>
    ///     Gets a list of <see cname="ProductInstance" /> objects linking this product to a group.
    /// </summary>
    public ICollection<ProductInstance> ProductInstances { get; set; } = new HashSet<ProductInstance>();

    /// <summary>
    ///     Get or set the name of the product.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    ///     Get or set a short description of the product.
    /// </summary>
    public string Description { get; set; } = default!;
}
