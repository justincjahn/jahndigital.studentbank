using System.Collections.Generic;
using System.Linq;

namespace jahndigital.studentbank.server
{
    /// <summary>
    /// Container class for application constants and built-in roles, privileges, etc.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Default values for authorization processes.
        /// </summary>
        public static class Auth
        {
            /// <summary>
            /// JWT Token Issuer
            /// </summary>
            public const string Issuer = "jahndigital.studentbank.server";

            /// <summary>
            /// The default expiration time, in minutes for a token.
            /// </summary>
            public const int DefaultExpirationMinutes = 60;

            /// <summary>
            /// Denotes the user type
            /// </summary>
            public const string CLAIM_USER_TYPE = "utyp";

            /// <summary>
            /// Denotes a preauthorization token.
            /// </summary>
            public const string CLAIM_PREAUTH_TYPE = "pre";

            public const string CLAIM_PREAUTH_YES = "Y";

            public const string CLAIM_PREAUTH_NO = "N";
        }

        /// <summary>
        /// Built-in privileges.
        /// </summary>
        public class Privilege
        {
            /// <summary>
            /// Gets the name of the privilege.
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// Gets a short description for the privilege.
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Returns the name of the privilege.
            /// </summary>
            /// <returns></returns>
            public override string ToString() => Name;

            /// <summary>
            /// Backing field for <see cname="Privileges" />.
            /// </summary>
            private static List<Privilege> _privileges = new List<Privilege>();

            /// <summary>
            /// Gets a list of privileges.
            /// </summary>
            public static IReadOnlyCollection<Privilege> Privileges { get => _privileges.AsReadOnly(); }

            /// <summary>
            /// Initialize the privilege and store it in the list.
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
            /// Allow all actions.
            /// </summary>
            public const string PRIVILEGE_ALL = "P_ALL";

            /// <summary>
            /// Allow all actions.
            /// </summary>
            public static readonly Privilege All = new Privilege(PRIVILEGE_ALL, "Allow all actions.");

            /// <summary>
            /// Create, update, and delete users.
            /// </summary>
            public const string PRIVILEGE_MANAGE_USERS = "P_MANAGE_USERS";

            /// <summary>
            /// Create, update, and delete users.
            /// </summary>
            public static readonly Privilege ManageUsers = new Privilege(PRIVILEGE_MANAGE_USERS, "Create, update, and delete users.");

            /// <summary>
            /// Create, update, and delete students.
            /// </summary>
            public const string PRIVILEGE_MANAGE_STUDENTS = "P_MANAGE_STUDENTS";

            /// <summary>
            /// Create, update, and delete students.
            /// </summary>
            public static readonly Privilege ManageStudents = new Privilege(PRIVILEGE_MANAGE_STUDENTS, "Create, update, and delete students.");

            /// <summary>
            /// Create, update, and delete student shares.
            /// </summary>
            public const string PRIVILEGE_MANAGE_SHARES = "P_MANAGE_SHARES";

            /// <summary>
            /// Create, update, and delete student shares.
            /// </summary>
            public static readonly Privilege ManageShares = new Privilege(PRIVILEGE_MANAGE_SHARES, "Create, update, and delete student shares.");

            /// <summary>
            /// Create, update, and delete share types.
            /// </summary>
            public const string PRIVILEGE_MANAGE_SHARE_TYPES = "P_MANAGE_SHARE_TYPES";

            /// <summary>
            /// Create, update, and delete share types.
            /// </summary>
            public static readonly Privilege ManageShareTypes = new Privilege(PRIVILEGE_MANAGE_SHARE_TYPES, "Create, update, and delete share types.");

            /// <summary>
            /// Post transactions to accounts.
            /// </summary>
            public const string PRIVILEGE_MANAGE_TRANSACTIONS = "P_MANAGE_TRANSACTIONS";

            /// <summary>
            /// Post transactions to accounts.
            /// </summary>
            public static readonly Privilege ManageTransactions = new Privilege(PRIVILEGE_MANAGE_TRANSACTIONS, "Post transactions to accounts.");

            /// <summary>
            /// Create, update, and delete stocks.
            /// </summary>
            public const string PRIVILEGE_MANAGE_STOCKS = "P_MANAGE_STOCKS";

            /// <summary>
            /// Create, update, and delete stocks.
            /// </summary>
            public static readonly Privilege ManageStocks = new Privilege(PRIVILEGE_MANAGE_STOCKS, "Create, update, and delete stocks.");

            /// <summary>
            /// Create, update, and delete groups.
            /// </summary>
            public const string PRIVILEGE_MANAGE_GROUPS = "P_MANAGE_GROUPS";

            /// <summary>
            /// Create, update, and delete groups.
            /// </summary>
            public static readonly Privilege ManageGroups = new Privilege(PRIVILEGE_MANAGE_GROUPS, "Create, update, and delete groups.");

            /// <summary>
            /// Create, update, and delete groups.
            /// </summary>
            public const string PRIVILEGE_MANAGE_INSTANCES = "P_MANAGE_INSTANCES";

            /// <summary>
            /// Create, update, and delete groups.
            /// </summary>
            public static readonly Privilege ManageInstances = new Privilege(PRIVILEGE_MANAGE_INSTANCES, "Create, update, and delete groups.");

            /// <summary>
            /// View and adjust student purchases.
            /// </summary>
            public const string PRIVILEGE_MANAGE_PURCHASES = "P_MANAGE_PURCHASES";

            /// <summary>
            /// View and adjust student purchases.
            /// </summary>
            public static readonly Privilege ManagePurchases = new Privilege(PRIVILEGE_MANAGE_PURCHASES, "View and adjust student purchases.");

            /// <summary>
            /// Create, update, and delete products.
            /// </summary>
            public const string PRIVILEGE_MANAGE_PRODUCTS = "P_MANAGE_PRODUCTS";

            /// <summary>
            /// Create, update, and delete products.
            /// </summary>
            public static readonly Privilege ManageProducts = new Privilege(PRIVILEGE_MANAGE_PRODUCTS, "Create, update, and delete products.");
        }

        /// <summary>
        /// Built-in roles.
        /// </summary>
        public class Role
        {
            /// <summary>
            /// Get the name of the role.
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// Gets a short description of the role.
            /// </summary>
            public string Description { get; private set; }

            /// <summary>
            /// List of privileges associated with this role.
            /// </summary>
            public IEnumerable<Privilege> Privileges { get; private set;}

            /// <summary>
            /// Backing field for <see cname="Roles" />
            /// </summary>
            private static List<Role> _roles = new List<Role>();

            /// <summary>
            /// Get every built-in role.
            /// </summary>
            public static IReadOnlyCollection<Role> Roles { get => _roles.AsReadOnly(); }

            /// <summary>
            /// Initialize a new Role and add it to the list of built-in roles.
            /// </summary>
            /// <param name="name">Friendly name for the role.</param>
            /// <param name="description">A short description of the role.</param>
            /// <param name="privileges">A list of privileges associated with this role.</param>
            private Role(string name, string description, IEnumerable<Privilege> privileges) {
                Name = name;
                Description = description;
                Privileges = privileges;
                _roles.Add(this);
            }

            /// <summary>
            /// Built-in role that represents the role with every permission.
            /// </summary>
            public const string ROLE_SUPERUSER = "Superuser";

            /// <summary>
            /// Built-in role that represents the role with every permission.
            /// </summary>
            public static readonly Role Superuser = new Role(
                ROLE_SUPERUSER,
                "Built-in role with all permissions.",
                new Privilege[] {
                    Privilege.All
                }
            );

            /// <summary>
            /// Built-in role with no administrative permissions.
            /// </summary>
            public const string ROLE_STUDENT = "Student";

            /// <summary>
            /// Built-in role with no administrative permissions.
            /// </summary>
            public static readonly Role Student = new Role(
                ROLE_STUDENT,
                "Built-in role with no administrative permissions.",
                new Privilege[] {}
            );
        }

        /// <summary>
        /// Policies for the ASP.NET Core Authorize functionality.
        /// </summary>
        public static class AuthPolicy
        {
            /// <summary>
            /// Requires that the user's token claim matches the userId URL parameter.
            /// </summary>
            public const string DataOwner = "DataOwner";

            /// <summary>
            /// Requires that the user's token have a preauthorization claim.
            /// </summary>
            public const string Preauthorization = "Preauthorization";
        }

        /// <summary>
        /// There can be multiple types of users in the application-- backend users and
        /// frontend users or students.  Backend users are consistent across all instances
        /// where frontend users may only interact within their own instance.
        /// </summary>
        public class UserType
        {
            /// <summary>
            /// Get the name of the user type.
            /// </summary>
            public string Name {get; private set;}

            /// <summary>
            /// Backing field for <see cref="UserTypes" />.
            /// </summary>
            private static List<UserType> _userTypes = new List<UserType>();

            /// <summary>
            /// Gets a list of user types.
            /// </summary>
            public static IReadOnlyCollection<UserType> UserTypes { get => _userTypes.AsReadOnly(); }

            private UserType(string name) {
                Name = name;
                _userTypes.Add(this);
            }

            public override string ToString() => Name;

            public static explicit operator UserType?(string value) =>
                UserTypes.FirstOrDefault(x => x.Name == value);

            /// <summary>
            /// A backend user
            /// </summary>
            public static readonly UserType User = new UserType("user");

            /// <summary>
            /// A frontend user
            /// </summary>
            public static readonly UserType Student = new UserType("student");
        }
    
        /// <summary>
        /// A list of error codes.
        /// </summary>
        public static class ErrorStrings
        {
            public const string ERROR_UNKNOWN = "ERROR_UNKNOWN";

            public const string ERROR_NOT_AUTHENTICATED = "AUTH_NOT_AUTHENTICATED";

            public const string ERROR_UNAUTHORIZED = "AUTH_NOT_AUTHORIZED";

            public const string ERROR_NOT_FOUND = "ERROR_NOT_FOUND";

            public const string ERROR_QUERY_FAILED = "ERROR_QUERY_FAILED";

            public const string INVALID_REFRESH_TOKEN = "INVALID_REFRESH_TOKEN";

            public const string TRANSACTION_NSF = "TRANSACTION_NSF";

            public const string TRANSACTION_WITHDRAWAL_LIMIT = "TRANSACTION_WITHDRAWAL_LIMIT";

            public const string TRANSACTION_STOCK_QUANTITY = "TRANSACTION_STOCK_QUANTITY";
        }
    }
}
