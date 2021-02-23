using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using jahndigital.studentbank.dal.Contexts;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    [ExtendObjectType(Name = "Query")]
    public class ShareTypeQueries
    {
        /// <summary>
        /// Get share type information available to the student or user.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UsePaging, UseSelection, UseSorting, UseFiltering, Authorize]
        public async Task<IQueryable<dal.Entities.ShareType>> GetAvailableShareTypesAsync(
            [Service]AppDbContext context,
            [Service]IResolverContext resolverContext
        ) {
            var userType = resolverContext.GetUserType() ?? throw ErrorFactory.Unauthorized();
            var userId = resolverContext.GetUserId() ?? throw ErrorFactory.Unauthorized();
            resolverContext.SetUser(userId, userType);

            if (userType == Constants.UserType.User) {
                return context.ShareTypes.Where(x => x.DateDeleted == null);
            }

            // Fetch the share type IDs the user has access to
            var shares = await context.Students
                .Include(x => x.Group)
                    .ThenInclude(x => x.Instance)
                        .ThenInclude(x => x.ShareTypeInstances)
                .Where(x => x.Id == userId)
                .FirstOrDefaultAsync()
            ?? throw ErrorFactory.NotFound();

            var shareTypeIds = shares.Group.Instance.ShareTypeInstances.Select(x => x.ShareTypeId);
            return context.ShareTypes.Where(x => x.DateDeleted == null && shareTypeIds.Contains(x.Id));
        }

        /// <summary>
        /// Get share types available for the given instance.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UsePaging, UseSelection, UseSorting, UseFiltering]
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public IQueryable<dal.Entities.ShareType> GetShareTypes(
            long instanceId,
            [Service]AppDbContext context
        ) {
            return context.ShareTypes
                .Include(x => x.ShareTypeInstances)
                .Where(x =>
                    x.ShareTypeInstances.Any(x => x.InstanceId == instanceId)
                    && x.DateDeleted == null);
        }

        /// <summary>
        /// Get share type information.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UsePaging, UseSelection, UseSorting, UseFiltering,
        Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public IQueryable<dal.Entities.ShareType> GetDeletedShareTypes([Service]AppDbContext context)
            => context.ShareTypes.Where(x => x.DateDeleted != null);
    }
}
