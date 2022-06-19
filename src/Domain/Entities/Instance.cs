using JahnDigital.StudentBank.Domain.Common;

namespace JahnDigital.StudentBank.Domain.Entities;

/// <summary>
///     Represents an instance of the software.  An instance is a collection of
///     groups, students, transactions, and other data.
/// </summary>
public class Instance : SoftDeletableEntity
{
    /// <summary>
    ///     The unique ID number of the entity.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     The description of the instance.
    /// </summary>
    public string Description { get; set; } = default!;

    /// <summary>
    ///     If the instance is currently active for user login.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     An invite code that is distributed to students for first-time registration.
    /// </summary>
    public string InviteCode { get; set; } = default!;

    /// <summary>
    ///     Gets or sets the collection of groups associated with this instance.
    /// </summary>
    public ICollection<Group> Groups { get; set; } = new HashSet<Group>();

    /// <summary>
    ///     Gets or sets the collection of stocks linked to this instance
    /// </summary>
    public ICollection<StockInstance> StockInstances { get; set; } = new HashSet<StockInstance>();

    /// <summary>
    ///     Get or set a collection of share types linked to this instance.
    /// </summary>
    public ICollection<ShareTypeInstance> ShareTypeInstances { get; set; } = new HashSet<ShareTypeInstance>();

    /// <summary>
    ///     Get or set a collection of Products linked to this instance.
    /// </summary>
    public ICollection<ProductInstance> ProductInstances { get; set; } = new HashSet<ProductInstance>();
}
