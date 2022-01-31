using System;
using System.Linq;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.utils;
using Microsoft.AspNetCore.Http;

namespace jahndigital.studentbank.server.GraphQL.Queries
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
            long id = resolverContext.GetUserId() ?? throw ErrorFactory.NotFound();
            Constants.UserType? type = resolverContext.GetUserType() ?? throw ErrorFactory.NotFound();

            if (type != Constants.UserType.User)
            {
                throw ErrorFactory.NotFound();
            }

            return context.Users.Where(x => x.Id == id);
        }

        /// <summary>
        ///     Returns true if the user is authenticated.
        /// </summary>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        public bool IsAuthenticated([Service] IHttpContextAccessor contextAccessor)
        {
            HttpContext? httpc = contextAccessor.HttpContext
                ?? throw new Exception("Unable to fetch HTTP Context to determine if user is authenticated.");

            return httpc.User.Identity?.IsAuthenticated ?? false;
        }
    }
}
