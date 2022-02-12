using JahnDigital.StudentBank.Domain.Common;

namespace JahnDigital.StudentBank.Domain.Entities;

/// <summary>
///     Represents a security event/privilege in the system.
/// </summary>
public class Privilege : AuditableEntity
{
    /// <summary>
    ///     The unique ID number of the entity.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     Unique name of the privilege.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    ///     A short description of the privilege.
    /// </summary>
    public string Description { get; set; } = default!;

    /// <summary>
    ///     Gets the intermediate table that links a Privilege to a collection or roles.
    /// </summary>
    public ICollection<RolePrivilege> RolePrivileges { get; set; } = new HashSet<RolePrivilege>();
}
