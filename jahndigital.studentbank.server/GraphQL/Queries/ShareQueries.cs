using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using jahndigital.studentbank.dal.Contexts;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    [ExtendObjectType(Name = "Query")]
    public class ShareQueries
    {
        /// <summary>
        /// Get shares for the currently active user.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UsePaging, UseSelection, UseSorting, UseFiltering, Authorize]
        public async Task<IQueryable<dal.Entities.Share>> GetShares(
            [Service]AppDbContext context,
            [Service]IResolverContext resolverContext
        ) {
            var userType = resolverContext.GetUserType() ?? throw ErrorFactory.Unauthorized();
            var userId = resolverContext.GetUserId() ?? throw ErrorFactory.Unauthorized();
            resolverContext.SetUser(userId, userType);

            if (userType == Constants.UserType.User) {
                var auth = await resolverContext.AuthorizeAsync(Constants.Privilege.ManageShares.Name);
                if (!auth.Succeeded) throw ErrorFactory.Unauthorized();
                return context.Shares.AsQueryable();
            }

            return context.Shares.Where(x => x.StudentId == userId);
        }
        
        /// <summary>
        /// Get deleted shares (if authorized).
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UsePaging, UseSelection, UseSorting, UseFiltering,
        Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_SHARES)]
        public IQueryable<dal.Entities.Share> GetDeletedShares([Service]AppDbContext context) =>
            context.Shares.Where(x => x.DateDeleted != null);
    }
}
