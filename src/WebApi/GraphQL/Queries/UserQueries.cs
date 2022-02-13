using System;
using System.Linq;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using JahnDigital.StudentBank.WebApi.Extensions;
using Microsoft.AspNetCore.Http;

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
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection,
         Authorize]
        public IQueryable<User> GetCurrentUser(
            [ScopedService] AppDbContext context,
            [Service] IResolverContext resolverContext
        )
        {
            if (resolverContext.GetUserType() != UserType.User) throw ErrorFactory.NotFound();

            return context
                .Users
                .Where(x => x.Id == resolverContext.GetUserId());
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
