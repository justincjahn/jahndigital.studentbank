using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Execution;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.server.Models;
using jahndigital.studentbank.services.DTOs;
using jahndigital.studentbank.services.Interfaces;
using jahndigital.studentbank.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using static jahndigital.studentbank.utils.Constants;

namespace jahndigital.studentbank.server.GraphQL.Mutations
{
    /// <summary>
    ///     CRUD operations for <see cref="dal.Entities.User" /> entities.
    /// </summary>
    [ExtendObjectType("Mutation")]
    public class UserMutations : TokenManagerAbstract
    {
        /// <summary>
        ///     Log the user in using a username and password and return JWT tokens.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="userService"></param>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        public async Task<AuthenticateResponse> UserLoginAsync(
            AuthenticateRequest input,
            [Service] IUserService userService,
            [Service] IHttpContextAccessor contextAccessor
        )
        {
            if (string.IsNullOrEmpty(input.Username))
            {
                throw ErrorFactory.Unauthorized();
            }

            if (string.IsNullOrEmpty(input.Password))
            {
                throw ErrorFactory.Unauthorized();
            }

            AuthenticateResponse? response = await userService.AuthenticateAsync(input, GetIp(contextAccessor))
                ?? throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("Bad username or password.")
                        .SetCode("LOGIN_FAIL")
                        .Build());

            SetTokenCookie(contextAccessor, response.RefreshToken);

            return response;
        }

        /// <summary>
        ///     Obtain a new JWT token using a refresh token.
        /// </summary>
        /// <param name="token">The refresh token to use when obtaining a new JWT token. Must be valid and not expired.</param>
        /// <param name="userService"></param>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        public async Task<AuthenticateResponse> UserRefreshTokenAsync(
            string? token,
            [Service] IUserService userService,
            [Service] IHttpContextAccessor contextAccessor
        )
        {
            token = token
                ?? GetToken(contextAccessor)
                ?? throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("No refresh token provided.")
                        .SetCode(ErrorStrings.INVALID_REFRESH_TOKEN)
                        .Build());

            AuthenticateResponse? response = await userService.RefreshTokenAsync(token, GetIp(contextAccessor))
                ?? throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("Invalid refresh token.")
                        .SetCode(ErrorStrings.INVALID_REFRESH_TOKEN)
                        .Build());

            SetTokenCookie(contextAccessor, response.RefreshToken);

            return response;
        }

        /// <summary>
        ///     Revoke a refresh token.
        /// </summary>
        /// <param name="token">The refresh token to revoke.</param>
        /// <param name="userService"></param>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        [HotChocolate.AspNetCore.Authorization.Authorize]
        public async Task<bool> UserRevokeRefreshTokenAsync(
            string? token,
            [Service] IUserService userService,
            [Service] IHttpContextAccessor contextAccessor
        )
        {
            token = token
                ?? GetToken(contextAccessor)
                ?? throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("A token is required.")
                        .SetCode(ErrorStrings.INVALID_REFRESH_TOKEN)
                        .Build());

            try
            {
                bool response = await userService.RevokeTokenAsync(token, GetIp(contextAccessor));

                if (!response)
                {
                    throw new QueryException(
                        ErrorBuilder.New()
                            .SetMessage("Token not found.")
                            .SetCode(ErrorStrings.ERROR_NOT_FOUND)
                            .Build()
                    );
                }

                ClearTokenCookie(contextAccessor);

                return response;
            }
            catch (QueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new QueryException(e.Message);
            }
        }

        /// <summary>
        ///     Update a user.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <param name="contextAccessor"></param>
        /// <param name="userService"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection, HotChocolate.AspNetCore.Authorization.Authorize]
        public async Task<IQueryable<User>> UpdateUserAsync(
            UpdateUserRequest input,
            [ScopedService] AppDbContext context,
            [Service] IResolverContext resolverContext,
            [Service] IHttpContextAccessor contextAccessor,
            [Service] IUserService userService
        )
        {
            resolverContext.SetUser(input.Id, UserType.User);
            AuthorizationResult? auth = await resolverContext.AuthorizeAsync(
                $"{AuthPolicy.DataOwner}<{Constants.Privilege.ManageUsers}>"
            );

            if (!auth.Succeeded)
            {
                throw ErrorFactory.Unauthorized();
            }

            User? user = await context.Users
                    .Where(x => x.Id == input.Id)
                    .SingleOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            // If the account belongs to the user, they must provide their current password
            if (user.Id == (resolverContext.GetUserId() ?? -1) && input.Password is not null)
            {
                if (input.CurrentPassword is null)
                {
                    throw ErrorFactory.Unauthorized();
                }

                AuthenticateRequest? req =
                    new AuthenticateRequest() { Username = user.Email, Password = input.CurrentPassword };

                AuthenticateResponse? res = await userService.AuthenticateAsync(req, GetIp(contextAccessor));

                if (res is null)
                {
                    throw new QueryException(
                        ErrorBuilder.New()
                            .SetMessage("Bad username or password.")
                            .SetCode("LOGIN_FAIL")
                            .Build());
                }
            }

            user.Email = input.Email ?? user.Email;
            user.RoleId = input.RoleId ?? user.RoleId;

            if (input.Password is not null)
            {
                user.Password = input.Password;
            }

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                throw ErrorFactory.QueryFailed(e.InnerException?.Message ?? e.Message);
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Users.Where(x => x.Id == user.Id);
        }

        /// <summary>
        ///     Create a new user.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)),
         HotChocolate.AspNetCore.Authorization.Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_USERS)]
        public async Task<IQueryable<User>> NewUserAsync(
            NewUserRequest input,
            [ScopedService] AppDbContext context
        )
        {
            bool roleExists = await context.Roles.Where(x => x.Id == input.RoleId).AnyAsync();

            if (!roleExists)
            {
                throw ErrorFactory.QueryFailed($"The Role ID ({input.RoleId}) does not exist.");
            }

            bool userExists = await context.Users.Where(x => x.Email == input.Email).AnyAsync();

            if (userExists)
            {
                throw ErrorFactory.QueryFailed($"A user with the email {input.Email} already exists.");
            }

            User? user = new User { Email = input.Email, RoleId = input.RoleId, Password = input.Password };

            try
            {
                context.Add(user);
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Users.Where(x => x.Id == user.Id);
        }

        /// <summary>
        ///     Delete a user.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)),
         HotChocolate.AspNetCore.Authorization.Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_USERS)]
        public async Task<bool> DeleteUserAsync(
            long id,
            [ScopedService] AppDbContext context,
            [Service] IResolverContext resolverContext
        )
        {
            User? user = await context.Users.FindAsync(id)
                ?? throw ErrorFactory.NotFound();

            long uid = resolverContext.GetUserId() ?? throw ErrorFactory.NotFound();

            if (id == uid)
            {
                throw ErrorFactory.QueryFailed("Cannot delete yourself!");
            }

            user.DateDeleted = DateTime.UtcNow;

            try
            {
                context.Update(user);
                await context.SaveChangesAsync();
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}