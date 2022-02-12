namespace JahnDigital.StudentBank.Domain.Common;

public class SoftDeletableEntity : AuditableEntity
{
    /// <summary>
    ///     Get or set the date the instance was deleted.
    /// </summary>
    public DateTime? DateDeleted { get; set; } = null;
}
