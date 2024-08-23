using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Application.Common.DTOs;
using JahnDigital.StudentBank.Application.Users.Commands.AuthenticateUser;
using JahnDigital.StudentBank.Application.Users.Commands.DeleteUser;
using JahnDigital.StudentBank.Application.Users.Commands.NewUser;
using JahnDigital.StudentBank.Application.Users.Commands.RefreshUserToken;
using JahnDigital.StudentBank.Application.Users.Commands.RevokeUserToken;
using JahnDigital.StudentBank.Application.Users.Commands.UpdateUser;
using JahnDigital.StudentBank.Application.Users.Queries.GetUser;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.WebApi.Extensions;
using JahnDigital.StudentBank.WebApi.Models;
using MediatR;
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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<AuthenticateResponse> UserLoginAsync(
            AuthenticateRequest input,
            [Service] ISender mediatr,
            [Service] IHttpContextAccessor contextAccessor,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrEmpty(input.Username) || string.IsNullOrEmpty(input.Password))
            {
                throw ErrorFactory.Unauthorized();
            }

            var command = new AuthenticateUserCommand(input.Username, input.Password, GetIp(contextAccessor));

            try
            {
                var response = await mediatr.Send(command, cancellationToken);
                SetTokenCookie(contextAccessor, response.RefreshToken);
                return response;
            }
            catch (Exception)
            {
                throw ErrorFactory.LoginFailed();
            }
        }

        /// <summary>
        ///     Obtain a new JWT token using a refresh token.
        /// </summary>
        /// <param name="token">The refresh token to use when obtaining a new JWT token. Must be valid and not expired.</param>
        /// <param name="mediatr"></param>
        /// <param name="contextAccessor"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<AuthenticateResponse> UserRefreshTokenAsync(
            string? token,
            [Service] ISender mediatr,
            [Service] IHttpContextAccessor contextAccessor,
            CancellationToken cancellationToken
        )
        {
            token ??= GetToken(contextAccessor) ?? throw ErrorFactory.InvalidRefreshToken();

            var command = new RefreshUserTokenCommand(token, GetIp(contextAccessor));

            try
            {
                var response = await mediatr.Send(command, cancellationToken);
                SetTokenCookie(contextAccessor, response.RefreshToken);
                return response;
            }
            catch (Exception)
            {
                throw ErrorFactory.InvalidRefreshToken();
            }
        }

        /// <summary>
        ///     Revoke a refresh token.
        /// </summary>
        /// <param name="token">The refresh token to revoke.</param>
        /// <param name="mediatr"></param>
        /// <param name="contextAccessor"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<bool> UserRevokeRefreshTokenAsync(
            string? token,
            [Service] ISender mediatr,
            [Service] IHttpContextAccessor contextAccessor,
            CancellationToken cancellationToken
        )
        {
            token ??= GetToken(contextAccessor) ?? throw ErrorFactory.InvalidRefreshToken();

            var command = new RevokeUserTokenCommand(token, GetIp(contextAccessor));

            try
            {
                await mediatr.Send(command, cancellationToken);
                ClearTokenCookie(contextAccessor);
                return true;
            }
            catch (Exception)
            {
                throw ErrorFactory.NotFound(nameof(token), token);
            }
        }

        /// <summary>
        ///     Update a user.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="resolverContext"></param>
        /// <param name="contextAccessor"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize, UseProjection]
        public async Task<IQueryable<User>> UpdateUserAsync(
            UpdateUserRequest input,
            [SchemaService] IResolverContext resolverContext,
            [Service] IHttpContextAccessor contextAccessor,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await resolverContext
                .SetDataOwner(input.Id, UserType.User)
                .AssertAuthorizedAsync($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageUsers}>");

            var user = await (await mediatr.Send(new GetUserQuery(input.Id), cancellationToken))
                .SingleOrDefaultAsync(cancellationToken)
            ?? throw ErrorFactory.NotFound(nameof(User), input.Id);

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
                    await mediatr.Send(command, cancellationToken);
                }
                catch
                {
                    throw ErrorFactory.LoginFailed();
                }
            }

            var updateUserCommand = new UpdateUserCommand(input.Id, input.RoleId, input.Email, input.Password);
            await mediatr.Send(updateUserCommand, cancellationToken);

            return await mediatr.Send(new GetUserQuery(input.Id), cancellationToken);
        }

        /// <summary>
        ///     Create a new user.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Policy = Privilege.PRIVILEGE_MANAGE_USERS)]
        public async Task<IQueryable<User>> NewUserAsync(
            NewUserRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            var id = await mediatr.Send(new NewUserCommand(input.RoleId, input.Email, input.Password), cancellationToken);
            return await mediatr.Send(new GetUserQuery(id), cancellationToken);
        }

        /// <summary>
        ///     Delete a user.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mediatr"></param>
        /// <param name="resolverContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Policy = Privilege.PRIVILEGE_MANAGE_USERS)]
        public async Task<bool> DeleteUserAsync(
            long id,
            [Service] ISender mediatr,
            [SchemaService] IResolverContext resolverContext,
            CancellationToken cancellationToken
        )
        {
            if (id == resolverContext.GetUserId())
            {
                throw ErrorFactory.QueryFailed("You cannot delete yourself!");
            }

            await mediatr.Send(new DeleteUserCommand(id), cancellationToken);

            return true;
        }
    }
}
