namespace JahnDigital.StudentBank.Domain.Enums;

/// <summary>
///     There can be multiple types of users in the application-- backend users and
///     frontend users or students.  Backend users are consistent across all instances
///     where frontend users may only interact within their own instance.
/// </summary>
public sealed class UserType : IComparable<UserType>
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

    public int CompareTo(UserType? other)
    {
        if (other is null) return 0;
        return String.Equals(other.Name, Name, StringComparison.CurrentCultureIgnoreCase) ? 1 : 0;
    }

    public override string ToString() => Name;

    public static explicit operator UserType?(string value) => UserTypes.FirstOrDefault(x => x.Name == value);
}
