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
        ///     Tell the client that the currently logged in user is unauthorized.
        /// </summary>
        /// <returns></returns>
        public static QueryException Unauthorized()
        {
            return new QueryException(
                ErrorBuilder.New()
                    .SetMessage("The current user is not authorized to access this resource.")
                    .SetCode(Constants.ErrorStrings.ERROR_UNAUTHORIZED)
                    .Build()
            );
        }

        /// <summary>
        ///     Tell the client that the resource they requested was not found.
        /// </summary>
        /// <returns></returns>
        public static QueryException NotFound()
        {
            return new QueryException(
                ErrorBuilder.New()
                    .SetMessage("The requested resource was not found.")
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
                ErrorBuilder.New()
                    .SetMessage(message)
                    .SetCode(Constants.ErrorStrings.ERROR_QUERY_FAILED)
                    .Build()
            );
        }
    }
}
