namespace JahnDigital.StudentBank.Domain.Common;

public abstract class AuditableEntity : EntityBase
{
    /// <summary>
    ///     Get the date the product was created.
    /// </summary>
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}
