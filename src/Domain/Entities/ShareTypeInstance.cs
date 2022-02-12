using JahnDigital.StudentBank.Domain.Common;

namespace JahnDigital.StudentBank.Domain.Entities;

/// <summary>
///     Table that joins one or more <see cref="ShareType" />s to one or more <see cref="Instance" />s.
/// </summary>
public class ShareTypeInstance : EntityBase
{
    /// <summary>
    ///     The ID number of the <see cref="ShareType" />
    /// </summary>
    public long ShareTypeId { get; set; }

    /// <summary>
    ///     The <see cref="Instance" /> ID.
    /// </summary>
    public long InstanceId { get; set; }

    /// <summary>
    ///     The <see cref="ShareType" /> assigned to the instance.
    /// </summary>
    public ShareType ShareType { get; set; } = default!;

    /// <summary>
    ///     The <see cref="Instance" /> the Share Type is linked to.
    /// </summary>
    public Instance Instance { get; set; } = default!;
}
