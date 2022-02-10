namespace JahnDigital.StudentBank.Domain.Enums;

/// <summary>
///     Built-in roles.
/// </summary>
public class Role
{
    /// <summary>
    ///     Built-in role that represents the role with every permission.
    /// </summary>
    public const string ROLE_SUPERUSER = "Superuser";

    /// <summary>
    ///     Built-in role with no administrative permissions.
    /// </summary>
    public const string ROLE_STUDENT = "Student";

    /// <summary>
    ///     Backing field for <see cname="Roles" />
    /// </summary>
    private static readonly List<Role> _roles = new();

    /// <summary>
    ///     Built-in role that represents the role with every permission.
    /// </summary>
    public static readonly Role Superuser = new(
        ROLE_SUPERUSER,
        "Built-in role with all permissions.",
        new[] { Privilege.All }
    );

    /// <summary>
    ///     Built-in role with no administrative permissions.
    /// </summary>
    public static readonly Role Student = new(
        ROLE_STUDENT,
        "Built-in role with no administrative permissions.",
        new Privilege[] { }
    );

    /// <summary>
    ///     Initialize a new Role and add it to the list of built-in roles.
    /// </summary>
    /// <param name="name">Friendly name for the role.</param>
    /// <param name="description">A short description of the role.</param>
    /// <param name="privileges">A list of privileges associated with this role.</param>
    private Role(string name, string description, IEnumerable<Privilege> privileges)
    {
        Name = name;
        Description = description;
        Privileges = privileges;
        _roles.Add(this);
    }

    /// <summary>
    ///     Get the name of the role.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets a short description of the role.
    /// </summary>
    public string Description { get; }

    /// <summary>
    ///     List of privileges associated with this role.
    /// </summary>
    public IEnumerable<Privilege> Privileges { get; }

    /// <summary>
    ///     Get every built-in role.
    /// </summary>
    public static IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();
}
