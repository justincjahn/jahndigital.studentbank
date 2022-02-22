using System;
using System.Linq;
using HotChocolate;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace JahnDigital.StudentBank.WebApi.GraphQL
{
    /// <summary>
    ///     Filters errors and translates them into more user-friendly messages.
    /// </summary>
    public class ErrorFilter : IErrorFilter
    {
        private readonly IHttpContextAccessor _context;

        public ErrorFilter(IHttpContextAccessor context)
        {
            _context = context;
        }

        public IError OnError(IError error)
        {
            Console.WriteLine($"[GraphQL Error] {error.Code}: {error.Exception?.Message ?? error.Message}");

            // If the use is unauthorized via the AuthorizeAttribute, determine if they are even
            // logged in and return a different error code if not.
            if (error.Code == Constants.ErrorStrings.ERROR_UNAUTHORIZED)
            {
                // Normally we throw an exception if we can't get an HTTPContext, but we're already in an error state.
                bool isAuth = _context.HttpContext?.User.Identity?.IsAuthenticated ?? false;

                if (!isAuth)
                {
                    return error
                        .WithCode(Constants.ErrorStrings.ERROR_NOT_AUTHENTICATED)
                        .WithMessage("User not authenticated.");
                }
            }

            if (error.Exception is NonsufficientFundsException)
            {
                return error
                    .WithCode(Constants.ErrorStrings.TRANSACTION_NSF)
                    .WithMessage(error.Exception.Message ?? "An invalid transaction occurred.");
            }

            if (error.Exception is WithdrawalLimitExceededException)
            {
                return error
                    .WithCode(Constants.ErrorStrings.TRANSACTION_WITHDRAWAL_LIMIT)
                    .WithMessage(error.Exception.Message ?? "An invalid transaction occurred.");
            }

            if (error.Exception is InvalidShareQuantityException)
            {
                return error
                    .WithCode(Constants.ErrorStrings.TRANSACTION_STOCK_QUANTITY)
                    .WithMessage(error.Exception.Message ?? "An invalid transaction occurred.");
            }

            if (error.Exception is ValidationException e)
            {
                // TODO: There can be multiple validation exceptions.  Figure out how to present them to the GQL client.
                var firstError = e.Errors.First().Value[0];

                return error
                    .WithCode("ERROR_INVALID_FIELD")
                    .WithMessage(firstError);
            }

            if (error.Exception is BaseException)
            {
                return error
                    .WithCode(Constants.ErrorStrings.ERROR_UNKNOWN)
                    .WithMessage(error.Exception.Message ?? error.Message);
            }

            if (error.Exception is not null)
            {
                return error
                    .WithCode(Constants.ErrorStrings.ERROR_UNKNOWN)
                    .WithMessage(error.Exception.Message ?? error.Message);
            }

            return error;
        }
    }
}
