namespace JahnDigital.StudentBank.Domain.Entities;

/// <summary>
///     Links a <see cref="Product" /> to a specific <see cref="Instance" />.
/// </summary>
public class ProductInstance
{
    /// <summary>
    ///     The ID number of the group.
    /// </summary>
    public long InstanceId { get; set; }

    /// <summary>
    ///     The ID number of the product.
    /// </summary>
    public long ProductId { get; set; }

    /// <summary>
    ///     Gets or sets the <see cname="Instance" /> a product has been released to.
    /// </summary>
    public Instance Instance { get; set; } = default!;

    /// <summary>
    ///     The Product associated with this link.
    /// </summary>
    public Product Product { get; set; } = default!;
}
