using System.Linq;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using jahndigital.studentbank.dal.Contexts;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    /// <summary>
    /// Operations around querying groups.
    /// </summary>
    [ExtendObjectType(Name = "Query")]
    public class GroupQueries
    {
        /// <summary>
        /// Get groups if authorized (Manage Groups).
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UsePaging, UseFiltering, UseSorting, UseSelection,
        Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_GROUPS)]
        public IQueryable<dal.Entities.Group> GetGroups([Service]AppDbContext context)
            => context.Groups.Where(x => x.DateDeleted == null);

        /// <summary>
        /// Get deleted groups if authorized (Manage Groups).
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UsePaging, UseFiltering, UseSorting, UseSelection,
        Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_GROUPS)]
        public IQueryable<dal.Entities.Group> GetDeletedGroups([Service]AppDbContext context)
            => context.Groups.Where(x => x.DateDeleted != null);
    }
}
