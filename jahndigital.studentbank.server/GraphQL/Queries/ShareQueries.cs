using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    [ExtendObjectType("Query")]
    public class ShareQueries
    {
        /// <summary>
        ///     Get shares for the currently active user.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         Authorize]
        public async Task<IQueryable<Share>> GetShares(
            [ScopedService] AppDbContext context,
            [Service] IResolverContext resolverContext
        )
        {
            var userType = resolverContext.GetUserType() ?? throw ErrorFactory.Unauthorized();
            var userId = resolverContext.GetUserId() ?? throw ErrorFactory.Unauthorized();
            resolverContext.SetUser(userId, userType);

            if (userType == Constants.UserType.User) {
                var auth = await resolverContext.AuthorizeAsync(Constants.Privilege.ManageShares.Name);

                if (!auth.Succeeded) {
                    throw ErrorFactory.Unauthorized();
                }

                return context.Shares.AsQueryable();
            }

            return context.Shares.Where(x => x.StudentId == userId);
        }

        /// <summary>
        ///     Get deleted shares (if authorized).
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_SHARES)]
        public IQueryable<Share> GetDeletedShares([ScopedService] AppDbContext context)
        {
            return context.Shares.Where(x => x.DateDeleted != null);
        }
    }
}
