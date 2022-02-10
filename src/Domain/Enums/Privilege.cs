namespace JahnDigital.StudentBank.Domain.Enums;

/// <summary>
///     Built-in privileges.
/// </summary>
public class Privilege
{
    /// <summary>
    ///     Allow all actions.
    /// </summary>
    public const string PRIVILEGE_ALL = "P_ALL";

    /// <summary>
    ///     Create, update, and delete users.
    /// </summary>
    public const string PRIVILEGE_MANAGE_USERS = "P_MANAGE_USERS";

    /// <summary>
    ///     Create, update, and delete students.
    /// </summary>
    public const string PRIVILEGE_MANAGE_STUDENTS = "P_MANAGE_STUDENTS";

    /// <summary>
    ///     Create, update, and delete student shares.
    /// </summary>
    public const string PRIVILEGE_MANAGE_SHARES = "P_MANAGE_SHARES";

    /// <summary>
    ///     Create, update, and delete share types.
    /// </summary>
    public const string PRIVILEGE_MANAGE_SHARE_TYPES = "P_MANAGE_SHARE_TYPES";

    /// <summary>
    ///     Post transactions to accounts.
    /// </summary>
    public const string PRIVILEGE_MANAGE_TRANSACTIONS = "P_MANAGE_TRANSACTIONS";

    /// <summary>
    ///     Create, update, and delete stocks.
    /// </summary>
    public const string PRIVILEGE_MANAGE_STOCKS = "P_MANAGE_STOCKS";

    /// <summary>
    ///     Create, update, and delete groups.
    /// </summary>
    public const string PRIVILEGE_MANAGE_GROUPS = "P_MANAGE_GROUPS";

    /// <summary>
    ///     Create, update, and delete groups.
    /// </summary>
    public const string PRIVILEGE_MANAGE_INSTANCES = "P_MANAGE_INSTANCES";

    /// <summary>
    ///     View and adjust student purchases.
    /// </summary>
    public const string PRIVILEGE_MANAGE_PURCHASES = "P_MANAGE_PURCHASES";

    /// <summary>
    ///     Create, update, and delete products.
    /// </summary>
    public const string PRIVILEGE_MANAGE_PRODUCTS = "P_MANAGE_PRODUCTS";

    /// <summary>
    ///     Backing field for <see cname="Privileges" />.
    /// </summary>
    private static readonly List<Privilege> _privileges = new();

    /// <summary>
    ///     Allow all actions.
    /// </summary>
    public static readonly Privilege All = new(PRIVILEGE_ALL, "Allow all actions.");

    /// <summary>
    ///     Create, update, and delete users.
    /// </summary>
    public static readonly Privilege ManageUsers =
        new(PRIVILEGE_MANAGE_USERS, "Create, update, and delete users.");

    /// <summary>
    ///     Create, update, and delete students.
    /// </summary>
    public static readonly Privilege ManageStudents =
        new(PRIVILEGE_MANAGE_STUDENTS, "Create, update, and delete students.");

    /// <summary>
    ///     Create, update, and delete student shares.
    /// </summary>
    public static readonly Privilege ManageShares =
        new(PRIVILEGE_MANAGE_SHARES, "Create, update, and delete student shares.");

    /// <summary>
    ///     Create, update, and delete share types.
    /// </summary>
    public static readonly Privilege ManageShareTypes =
        new(PRIVILEGE_MANAGE_SHARE_TYPES, "Create, update, and delete share types.");

    /// <summary>
    ///     Post transactions to accounts.
    /// </summary>
    public static readonly Privilege ManageTransactions =
        new(PRIVILEGE_MANAGE_TRANSACTIONS, "Post transactions to accounts.");

    /// <summary>
    ///     Create, update, and delete stocks.
    /// </summary>
    public static readonly Privilege ManageStocks =
        new(PRIVILEGE_MANAGE_STOCKS, "Create, update, and delete stocks.");

    /// <summary>
    ///     Create, update, and delete groups.
    /// </summary>
    public static readonly Privilege ManageGroups =
        new(PRIVILEGE_MANAGE_GROUPS, "Create, update, and delete groups.");

    /// <summary>
    ///     Create, update, and delete groups.
    /// </summary>
    public static readonly Privilege ManageInstances =
        new(PRIVILEGE_MANAGE_INSTANCES, "Create, update, and delete groups.");

    /// <summary>
    ///     View and adjust student purchases.
    /// </summary>
    public static readonly Privilege ManagePurchases =
        new(PRIVILEGE_MANAGE_PURCHASES, "View and adjust student purchases.");

    /// <summary>
    ///     Create, update, and delete products.
    /// </summary>
    public static readonly Privilege ManageProducts =
        new(PRIVILEGE_MANAGE_PRODUCTS, "Create, update, and delete products.");

    /// <summary>
    ///     Initialize the privilege and store it in the list.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    private Privilege(string name, string description)
    {
        Name = name;
        Description = description;
        _privileges.Add(this);
    }

    /// <summary>
    ///     Gets the name of the privilege.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets a short description for the privilege.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     Gets a list of privileges.
    /// </summary>
    public static IReadOnlyCollection<Privilege> Privileges => _privileges.AsReadOnly();

    /// <summary>
    ///     Returns the name of the privilege.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Name;
    }
}
