using System.Linq;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;
using Microsoft.AspNetCore.Http;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    /// <summary>
    /// 
    /// </summary>
    [ExtendObjectType(Name = "Query")]
    public class UserQueries
    {
        /// <summary>
        /// Get the currently logged in user information (if the user is a user).
        /// </summary>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [Authorize]
        public IQueryable<dal.Entities.User> GetCurrentUser(
            [Service] AppDbContext context,
            [Service] IResolverContext resolverContext
        ) {
            var id = resolverContext.GetUserId() ?? throw ErrorFactory.NotFound();
            var type = resolverContext.GetUserType() ?? throw ErrorFactory.NotFound();
            if (type != Constants.UserType.User) throw ErrorFactory.NotFound();
            return context.Users.Where(x => x.Id == id);
        }

        /// <summary>
        /// Returns true if the user is authenticated.
        /// </summary>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        public bool IsAuthenticated([Service] IHttpContextAccessor contextAccessor)
            => contextAccessor.HttpContext.User.Identity.IsAuthenticated;
    }
}
