using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Users.Queries.GetUser;
using JahnDigital.StudentBank.Application.Users.Queries.GetUsers;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using JahnDigital.StudentBank.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Queries
{
    /// <summary>
    /// </summary>
    [ExtendObjectType("Query")]
    public class UserQueries
    {
        /// <summary>
        ///     Get the currently logged in user information (if the user is a user).
        /// </summary>
        /// <param name="resolverContext"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize]
        public Task<IQueryable<User>> GetCurrentUserAsync(
            [Service] IResolverContext resolverContext,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            if (resolverContext.GetUserType() != UserType.User) throw ErrorFactory.NotFound();
            return mediatr.Send(new GetUserQuery(resolverContext.GetUserId()), cancellationToken);
        }

        /// <summary>
        ///     Gets a list of users in the system.
        /// </summary>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UsePaging, UseProjection, UseFiltering, UseSorting, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_USERS)]
        public Task<IQueryable<User>> GetUsersAsync(
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            return mediatr.Send(new GetUsersQuery(), cancellationToken);
        }

        /// <summary>
        ///     Returns true if the user is authenticated.
        /// </summary>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        public bool IsAuthenticated([Service] IHttpContextAccessor contextAccessor)
        {
            return (contextAccessor.HttpContext?.GetUserId() ?? -1) > 0;
        }
    }
}
