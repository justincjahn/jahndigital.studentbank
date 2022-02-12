using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Execution;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Application.Common.DTOs;
using JahnDigital.StudentBank.Application.Users.Commands.AuthenticateUser;
using JahnDigital.StudentBank.Application.Users.Commands.RefreshUserToken;
using JahnDigital.StudentBank.Application.Users.Commands.RevokeUserToken;
using JahnDigital.StudentBank.Application.Users.Commands.UpdateUser;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using JahnDigital.StudentBank.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Mutations
{
    /// <summary>
    ///     CRUD operations for <see cref="User" /> entities.
    /// </summary>
    [ExtendObjectType("Mutation")]
    public class UserMutations : TokenManagerAbstract
    {
        /// <summary>
        ///     Log the user in using a username and password and return JWT tokens.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        public async Task<AuthenticateResponse> UserLoginAsync(
            AuthenticateRequest input,
            [Service] ISender mediatr,
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

            var command = new AuthenticateUserCommand(input.Username, input.Password, GetIp(contextAccessor));

            try
            {
                var response = await mediatr.Send(command);
                SetTokenCookie(contextAccessor, response.RefreshToken);
                return response;
            }
            catch (Exception)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("Bad username or password.")
                        .SetCode("LOGIN_FAIL")
                        .Build());
            }
        }

        /// <summary>
        ///     Obtain a new JWT token using a refresh token.
        /// </summary>
        /// <param name="token">The refresh token to use when obtaining a new JWT token. Must be valid and not expired.</param>
        /// <param name="mediatr"></param>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        public async Task<AuthenticateResponse> UserRefreshTokenAsync(
            string? token,
            [Service] ISender mediatr,
            [Service] IHttpContextAccessor contextAccessor
        )
        {
            token = token
                ?? GetToken(contextAccessor)
                ?? throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("No refresh token provided.")
                        .SetCode(Constants.ErrorStrings.INVALID_REFRESH_TOKEN)
                        .Build());

            var command = new RefreshUserTokenCommand(token, GetIp(contextAccessor));

            try
            {
                var response = await mediatr.Send(command);
                SetTokenCookie(contextAccessor, response.RefreshToken);
                return response;
            }
            catch (Exception)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("Invalid refresh token.")
                        .SetCode(Constants.ErrorStrings.INVALID_REFRESH_TOKEN)
                        .Build());
            }
        }

        /// <summary>
        ///     Revoke a refresh token.
        /// </summary>
        /// <param name="token">The refresh token to revoke.</param>
        /// <param name="mediatr"></param>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        [HotChocolate.AspNetCore.Authorization.Authorize]
        public async Task<bool> UserRevokeRefreshTokenAsync(
            string? token,
            [Service] ISender mediatr,
            [Service] IHttpContextAccessor contextAccessor
        )
        {
            token = token
                ?? GetToken(contextAccessor)
                ?? throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("A token is required.")
                        .SetCode(Constants.ErrorStrings.INVALID_REFRESH_TOKEN)
                        .Build());

            try
            {
                var command = new RevokeUserTokenCommand(token, GetIp(contextAccessor));
                await mediatr.Send(command);
                ClearTokenCookie(contextAccessor);
                return true;
            }
            catch (Exception)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("Token not found.")
                        .SetCode(Constants.ErrorStrings.ERROR_NOT_FOUND)
                        .Build()
                );
            }
        }

        /// <summary>
        ///     Update a user.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <param name="contextAccessor"></param>
        /// <param name="mediatr"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection, HotChocolate.AspNetCore.Authorization.Authorize]
        public async Task<IQueryable<User>> UpdateUserAsync(
            UpdateUserRequest input,
            [ScopedService] AppDbContext context,
            [Service] IResolverContext resolverContext,
            [Service] IHttpContextAccessor contextAccessor,
            [Service] ISender mediatr
        )
        {
            resolverContext.SetUser(input.Id, UserType.User);
            AuthorizationResult? auth = await resolverContext.AuthorizeAsync(
                $"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageUsers}>"
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
            if (user.Id == resolverContext.GetUserId() && input.Password is not null)
            {
                if (input.CurrentPassword is null)
                {
                    throw ErrorFactory.Unauthorized();
                }

                // TODO: ValidateUserPasswordCommand, don't query for the user in the mutation itself
                var command = new AuthenticateUserCommand(user.Email, input.CurrentPassword, GetIp(contextAccessor));

                try
                {
                    var response = await mediatr.Send(command);
                }
                catch
                {
                    throw new QueryException(
                        ErrorBuilder.New()
                            .SetMessage("Bad username or password.")
                            .SetCode("LOGIN_FAIL")
                            .Build());
                }
            }

            var updateUserCommand = new UpdateUserCommand(input.Id, input.RoleId, input.Email, input.Password);

            try
            {
                await mediatr.Send(updateUserCommand);

                return context.Users.Where(x => x.Id == user.Id);
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.InnerException?.Message ?? e.Message);
            }
        }

        /// <summary>
        ///     Create a new user.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)),
         HotChocolate.AspNetCore.Authorization.Authorize(Policy = Privilege.PRIVILEGE_MANAGE_USERS)]
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
         HotChocolate.AspNetCore.Authorization.Authorize(Policy = Privilege.PRIVILEGE_MANAGE_USERS)]
        public async Task<bool> DeleteUserAsync(
            long id,
            [ScopedService] AppDbContext context,
            [Service] IResolverContext resolverContext
        )
        {
            User? user = await context.Users.FindAsync(id)
                ?? throw ErrorFactory.NotFound();

            if (id == resolverContext.GetUserId())
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
