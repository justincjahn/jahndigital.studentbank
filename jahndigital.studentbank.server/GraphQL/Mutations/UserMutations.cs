using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Execution;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.server.Models;
using jahndigital.studentbank.server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using static jahndigital.studentbank.server.Constants;

namespace jahndigital.studentbank.server.GraphQL.Mutations
{
    /// <summary>
    /// CRUD operations for <see cref="dal.Entities.User"/> entities.
    /// </summary>
    [ExtendObjectType(Name = "Mutation")]
    public class UserMutations : TokenManagerAbstract
    {
        /// <summary>
        /// Log the user in using a username and password and return JWT tokens.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="userService"></param>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        public AuthenticateResponse UserLogin(
            AuthenticateRequest input,
            [Service] IUserService userService,
            [Service] IHttpContextAccessor contextAccessor
        ) {
            if (string.IsNullOrEmpty(input.Username)) throw ErrorFactory.Unauthorized();
            if (string.IsNullOrEmpty(input.Password)) throw ErrorFactory.Unauthorized();

            var response = userService.Authenticate(input, GetIp(contextAccessor))
                ?? throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("Bad username or password.")
                        .SetCode("LOGIN_FAIL")
                        .Build());

            SetTokenCookie(contextAccessor, response.RefreshToken);
            return response;
        }

        /// <summary>
        /// Obtain a new JWT token using a refresh token.
        /// </summary>
        /// <param name="token">The refresh token to use when obtaining a new JWT token. Must be valid and not expired.</param>
        /// <param name="userService"></param>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        public AuthenticateResponse UserRefreshToken(
            string? token,
            [Service] IUserService userService,
            [Service] IHttpContextAccessor contextAccessor
        ) {
            token = token
                ?? GetToken(contextAccessor)
                ?? throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("No refresh token provided.")
                        .SetCode(Constants.ErrorStrings.INVALID_REFRESH_TOKEN)
                        .Build());

            var response = userService.RefreshToken(token, GetIp(contextAccessor))
                ?? throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("Invalid refresh token.")
                        .SetCode(Constants.ErrorStrings.INVALID_REFRESH_TOKEN)
                        .Build());

            SetTokenCookie(contextAccessor, response.RefreshToken);
            return response;
        }

        /// <summary>
        /// Revoke a refresh token.
        /// </summary>
        /// <param name="token">The refresh token to revoke.</param>
        /// <param name="userService"></param>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        [Authorize]
        public bool UserRevokeRefreshToken(
            string? token,
            [Service] IUserService userService,
            [Service] IHttpContextAccessor contextAccessor
        ) {
            token = token
                ?? GetToken(contextAccessor)
                ?? throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("A token is required.")
                        .SetCode(Constants.ErrorStrings.INVALID_REFRESH_TOKEN)
                        .Build());

            var response = userService.RevokeToken(token, GetIp(contextAccessor));

            if (!response) {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("Token not found.")
                        .SetCode(ErrorStrings.ERROR_NOT_FOUND)
                        .Build()
                );
            }

            return response;
        }
    
        /// <summary>
        /// Update a user.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseSelection, Authorize]
        public async Task<IQueryable<dal.Entities.User>> UpdateUserAsync(
            UpdateUserRequest input,
            [Service] AppDbContext context,
            [Service] IResolverContext resolverContext
        ) {
            resolverContext.SetUser(input.Id, UserType.User);
            var auth = await resolverContext.AuthorizeAsync(
                $"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManageUsers}>"
            );

            if (!auth.Succeeded) throw ErrorFactory.Unauthorized();

            var user = await context.Users
                .Where(x => x.Id == input.Id)
                .SingleOrDefaultAsync()
            ?? throw ErrorFactory.NotFound();

            user.Email = input.Email ?? user.Email;
            user.RoleId = input.RoleId ?? user.RoleId;
            if (input.Password != null) user.Password = input.Password;

            try {
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Users.Where(x => x.Id == user.Id);
        }

        /// <summary>
        /// Create a new user.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_USERS)]
        public async Task<IQueryable<dal.Entities.User>> NewUserAsync(
            NewUserRequest input,
            [Service] AppDbContext context
        ) {
            var roleExists = await context.Roles.Where(x => x.Id == input.RoleId).AnyAsync();
            if (!roleExists) throw ErrorFactory.QueryFailed($"The Role ID ({input.RoleId}) does not exist.");

            var userExists = await context.Users.Where(x => x.Email == input.Email).AnyAsync();
            if (userExists) throw ErrorFactory.QueryFailed($"A user with the email {input.Email} already exists.");
            
            var user = new dal.Entities.User {
                Email = input.Email,
                RoleId = input.RoleId,
                Password = input.Password
            };

            try {
                context.Add(user);
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Users.Where(x => x.Id == user.Id);
        }
    
        /// <summary>
        /// Delete a user.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_USERS)]
        public async Task<bool> DeleteUserAsync(
            long id,
            [Service] AppDbContext context,
            [Service] IResolverContext resolverContext
        ) {
            var user = await context.Users.FindAsync(id)
                ?? throw ErrorFactory.NotFound();

            var uid = resolverContext.GetUserId() ?? throw ErrorFactory.NotFound();

            if (id == uid) {
                throw ErrorFactory.QueryFailed("Cannot delete yourself!");
            }

            user.DateDeleted = DateTime.UtcNow;

            try {
                context.Update(user);
                await context.SaveChangesAsync();
            } catch {
                return false;
            }

            return true;
        }
    }
}
