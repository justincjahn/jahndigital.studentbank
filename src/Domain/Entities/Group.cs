using JahnDigital.StudentBank.Domain.Common;

namespace JahnDigital.StudentBank.Domain.Entities;

/// <summary>
///     A group represents a collection of Students, usually as classes.
/// </summary>
public class Group : SoftDeletableEntity
{
    /// <summary>
    ///     The unique ID number of the entity.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     Get the Instance ID.
    /// </summary>
    public long InstanceId { get; set; }

    /// <summary>
    ///     Name of the group.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    ///     The instance of the group.
    /// </summary>
    public Instance Instance { get; set; } = default!;

    /// <summary>
    ///     Gets the list of students associated with this group.
    /// </summary>
    public ICollection<Student> Students { get; set; } = new HashSet<Student>();
}
