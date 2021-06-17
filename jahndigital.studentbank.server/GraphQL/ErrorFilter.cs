using System;
using HotChocolate;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Http;
using jahndigital.studentbank.server.Exceptions;

namespace jahndigital.studentbank.server.GraphQL
{
    /// <summary>
    /// Filters errors and translates them into more user-friendly messages.
    /// </summary>
    public class ErrorFilter : IErrorFilter
    {
        private readonly IHttpContextAccessor _context;

        public ErrorFilter(IHttpContextAccessor context) => this._context = context;

        public IError OnError(IError error)
        {
            Console.WriteLine($"[GraphQL Error] {error.Code}: {error.Message}.");

            // If the use is unauthorized via the AuthorizeAttribute, determine if they are even
            // logged in and return a different error code if not.
            if (error.Code == Constants.ErrorStrings.ERROR_UNAUTHORIZED) {
                // Normally we throw an exception if we can't get an HTTPContext, but we're already in an error state.
                var isAuth = _context.HttpContext?.User.Identity?.IsAuthenticated ?? false;

                if (!isAuth) {
                    return error
                        .WithCode(Constants.ErrorStrings.ERROR_NOT_AUTHENTICATED)
                        .WithMessage("User not authenticated.");
                }
            }

            if (error.Exception is NonsufficientFundsException) {
                return error
                    .WithCode(Constants.ErrorStrings.TRANSACTION_NSF)
                    .WithMessage(error.Exception.Message);
            }

            if (error.Exception is WithdrawalLimitExceededException) {
                return error
                    .WithCode(Constants.ErrorStrings.TRANSACTION_WITHDRAWAL_LIMIT)
                    .WithMessage(error.Exception.Message);
            }

            if (error.Exception is InvalidShareQuantityException) {
                return error
                    .WithCode(Constants.ErrorStrings.TRANSACTION_STOCK_QUANTITY)
                    .WithMessage(error.Exception.Message);
            }
            
            if (error.Exception is BaseException) {
                return error
                    .WithCode(Constants.ErrorStrings.ERROR_UNKNOWN)
                    .WithMessage(error.Exception.Message);
            }

            return error;
        }
    }
}
