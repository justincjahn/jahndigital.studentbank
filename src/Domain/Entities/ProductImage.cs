using JahnDigital.StudentBank.Domain.Common;

namespace JahnDigital.StudentBank.Domain.Entities;

/// <summary>
///     Represents a list of images for a given product.
/// </summary>
public class ProductImage : EntityBase
{
    /// <summary>
    ///     The unique ID number of the entity.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     Gets the image URL.
    /// </summary>
    public string Url { get; set; } = default!;
}
