using System;
using System.Threading;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.server.Models;
using jahndigital.studentbank.server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace jahndigital.studentbank.server.GraphQL.Mutations
{
    [ExtendObjectType(Name = "Mutation")]
    public class UserMutations : TokenManagerAbstract
    {
        /// <summary>
        /// Log the user in using a username and password and return JWT tokens.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="userService"></param>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public AuthenticateResponse UserLogin(
            AuthenticateRequest request,
            [Service] AppDbContext context,
            [Service] IUserService userService,
            [Service] IHttpContextAccessor contextAccessor
        ) {
            if (string.IsNullOrEmpty(request.Username)) {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("The username can't be empty.")
                        .SetCode("USERNAME_EMPTY")
                        .Build()
                );
            }

            if (string.IsNullOrEmpty(request.Password)) {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("The password can't be empty.")
                        .SetCode("PASSWORD_EMPTY")
                        .Build()
                );
            }

            var response = userService.Authenticate(request, getIp(contextAccessor));

            if (response == null) {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("Bad username or password.")
                        .SetCode("LOGIN_FAIL")
                        .Build()
                );
            }

            setTokenCookie(contextAccessor, response.RefreshToken);
            return response!;
        }

        /// <summary>
        /// Obtain a new JWT token using a refresh token.
        /// </summary>
        /// <param name="token">The refresh token to use when obtaining a new JWT token. Must be valid and not expired.</param>
        /// <param name="context"></param>
        /// <param name="userService"></param>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public AuthenticateResponse? UserRefreshToken(
            string token,
            [Service] AppDbContext context,
            [Service] IUserService userService,
            [Service] IHttpContextAccessor contextAccessor
        ) {
            var response = userService.RefreshToken(token, getIp(contextAccessor));

            if (response == null) {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("Invalid refresh token.")
                        .SetCode("INVALID_REFRESH_TOKEN")
                        .Build()
                );
            }

            setTokenCookie(contextAccessor, response.RefreshToken);
            return response;
        }

        /// <summary>
        /// Revoke a refresh token.
        /// </summary>
        /// <param name="token">The refresh token to revoke.</param>
        /// <param name="context"></param>
        /// <param name="userService"></param>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        [Authorize]
        public bool UserRevokeRefreshToken(
            string token,
            [Service] AppDbContext context,
            [Service] IUserService userService,
            [Service] IHttpContextAccessor contextAccessor
        ) {
            if (string.IsNullOrWhiteSpace(token)) {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("A token is required.")
                        .SetCode("TOKEN_EMPTY")
                        .Build()
                );
            }

            var response = userService.RevokeToken(token, getIp(contextAccessor));

            if (!response) {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("Token not found.")
                        .SetCode("TOKEN_NOT_FOUND")
                        .Build()
                );
            }

            return response;
        }
    }
}
