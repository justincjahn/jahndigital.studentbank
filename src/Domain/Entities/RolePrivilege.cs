using JahnDigital.StudentBank.Domain.Common;

namespace JahnDigital.StudentBank.Domain.Entities;

/// <summary>
///     Table that joins one or more roles to one or more privileges.
/// </summary>
public class RolePrivilege : EntityBase
{
    /// <summary>
    ///     Get or set the ID number of the role.
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    ///     Get or set the ID number of the privilege.
    /// </summary>
    public long PrivilegeId { get; set; }

    /// <summary>
    ///     The Role associated with this privilege.
    /// </summary>
    public Role Role { get; set; } = default!;

    /// <summary>
    ///     The Privilege associated with this role.
    /// </summary>
    public Privilege Privilege { get; set; } = default!;
}
