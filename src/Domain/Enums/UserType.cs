namespace JahnDigital.StudentBank.Domain.Enums;

/// <summary>
///     There can be multiple types of users in the application-- backend users and
///     frontend users or students.  Backend users are consistent across all instances
///     where frontend users may only interact within their own instance.
/// </summary>
public sealed class UserType
{
    /// <summary>
    ///     Backing field for <see cref="UserTypes" />.
    /// </summary>
    private static readonly List<UserType> _userTypes = new();

    /// <summary>
    ///     A backend user
    /// </summary>
    public static readonly UserType User = new("user");

    /// <summary>
    ///     A frontend user
    /// </summary>
    public static readonly UserType Student = new("student");

    /// <summary>
    ///     An anonymous user
    /// </summary>
    public static readonly UserType Anonymous = new("anonymous");

    private UserType(string name)
    {
        Name = name;
        _userTypes.Add(this);
    }

    /// <summary>
    ///     Get the name of the user type.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets a list of user types.
    /// </summary>
    public static IReadOnlyCollection<UserType> UserTypes => _userTypes.AsReadOnly();

    public override string ToString()
    {
        return Name;
    }

    public static explicit operator UserType?(string value)
    {
        return UserTypes.FirstOrDefault(x => x.Name == value);
    }
}
