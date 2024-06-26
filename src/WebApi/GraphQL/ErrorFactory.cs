using HotChocolate;
using HotChocolate.Execution;
using JahnDigital.StudentBank.Application.Common;

namespace JahnDigital.StudentBank.WebApi.GraphQL
{
    /// <summary>
    ///     A factory object to produce various HotChocolate error messages.
    /// </summary>
    /// <remarks>
    ///     Use an <see cname="IErrorFilter" /> instead.
    ///     https://josiahmortenson.dev/blog/2020-06-05-hotchocolate-graphql-errors
    /// </remarks>
    public static class ErrorFactory
    {
        /// <summary>
        ///     Tell the client that the login attempt has failed.
        /// </summary>
        /// <returns></returns>
        public static QueryException LoginFailed()
        {
            return new QueryException(
                ErrorBuilder
                    .New()
                    .SetMessage("Bad username or password.")
                    .SetCode(Constants.ErrorStrings.ERROR_LOGIN_FAILED)
                    .Build()
            );
        }

        /// <summary>
        ///     Tell the client that the currently logged in user is unauthorized.
        /// </summary>
        /// <returns></returns>
        public static QueryException Unauthorized()
        {
            return new QueryException(
                ErrorBuilder
                    .New()
                    .SetMessage("The current user is not authorized to access this resource.")
                    .SetCode(Constants.ErrorStrings.ERROR_UNAUTHORIZED)
                    .Build()
            );
        }

        /// <summary>
        ///     Tell the client that the provided refresh token is invalid or expired.
        /// </summary>
        /// <returns></returns>
        public static QueryException InvalidRefreshToken()
        {
            return new QueryException(
                ErrorBuilder
                    .New()
                    .SetMessage("A token is required.")
                    .SetCode(Constants.ErrorStrings.INVALID_REFRESH_TOKEN)
                    .Build()
            );
        }

        /// <summary>
        ///     Tell the client that the provided invite code/account number combination was invalid.
        /// </summary>
        /// <param name="inviteCode"></param>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        public static QueryException InvalidInviteCode(string inviteCode = "", string accountNumber = "")
        {
            return new QueryException(
                ErrorBuilder
                    .New()
                    .SetMessage("Invalid invite code or account number.")
                    .SetExtension("inviteCode", inviteCode)
                    .SetExtension("accountNumber", accountNumber)
                    .SetCode(Constants.ErrorStrings.ERROR_NOT_FOUND)
                    .Build()
            );
        }

        /// <summary>
        ///     Tell the client that the resource they requested was not found.
        /// </summary>
        /// <returns></returns>
        public static QueryException NotFound(string objectName = "", object? key = null)
        {
            var msg = "The requested resource was not found.";

            if (!string.IsNullOrWhiteSpace(objectName))
            {
                msg = $"Resource \"{objectName}\" ({key}) was not found.";
            }

            return new QueryException(
                ErrorBuilder
                    .New()
                    .SetMessage(msg)
                    .SetExtension("resource", objectName)
                    .SetExtension("key", key)
                    .SetCode(Constants.ErrorStrings.ERROR_NOT_FOUND)
                    .Build()
            );
        }

        /// <summary>
        ///     A database query failed.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static QueryException QueryFailed(string message)
        {
            return new QueryException(
                ErrorBuilder
                    .New()
                    .SetMessage(message)
                    .SetCode(Constants.ErrorStrings.ERROR_QUERY_FAILED)
                    .Build()
            );
        }
    }
}
