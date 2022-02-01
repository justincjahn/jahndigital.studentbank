namespace JahnDigital.StudentBank.Domain.Entities;

/// <summary>
///     Represents a list of images for a given product.
/// </summary>
public class ProductImage
{
    /// <summary>
    ///     Gets the unique ID of the record.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     Gets the image URL.
    /// </summary>
    public string Url { get; set; } = default!;
}
