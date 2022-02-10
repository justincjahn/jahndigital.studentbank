namespace JahnDigital.StudentBank.Application.Common;

/// <summary>
///     Container class for application constants and built-in roles, privileges, etc.
/// </summary>
public static class Constants
{
    /// <summary>
    ///     Default values for authorization processes.
    /// </summary>
    public static class Auth
    {
        /// <summary>
        ///     JWT Token Issuer
        /// </summary>
        public const string Issuer = "jahndigital.studentbank.server";

        /// <summary>
        ///     The default expiration time, in minutes for a token.
        /// </summary>
        public const int DefaultExpirationMinutes = 60;

        /// <summary>
        ///     Denotes the user type
        /// </summary>
        public const string CLAIM_USER_TYPE = "utyp";

        /// <summary>
        ///     Denotes a preauthorization token.
        /// </summary>
        public const string CLAIM_PREAUTH_TYPE = "pre";

        /// <summary>
        ///     Value that represents preauthorization true
        /// </summary>
        public const string CLAIM_PREAUTH_YES = "Y";

        /// <summary>
        ///     Value that represents preauthorization false
        /// </summary>
        public const string CLAIM_PREAUTH_NO = "N";
    }

    /// <summary>
    ///     Policies for the ASP.NET Core Authorize functionality.
    /// </summary>
    public static class AuthPolicy
    {
        /// <summary>
        ///     Requires that the user's token claim matches the userId URL parameter.
        /// </summary>
        public const string DataOwner = "DataOwner";

        /// <summary>
        ///     Requires that the user's token have a preauthorization claim.
        /// </summary>
        public const string Preauthorization = "Preauthorization";
    }

    /// <summary>
    ///     A list of error codes.
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
