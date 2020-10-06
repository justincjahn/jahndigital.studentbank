using System.Linq;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using jahndigital.studentbank.dal.Contexts;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    [ExtendObjectType(Name = "Query")]
    public class ShareTypeQueries
    {
        /// <summary>
        /// Get share type information.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseSelection]
        public IQueryable<dal.Entities.ShareType> GetShareTypes([Service]AppDbContext context)
            => context.ShareTypes.Where(x => x.DateDeleted == null);
        
        /// <summary>
        /// Get share type information.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UsePaging, UseFiltering, UseSorting, UseSelection,
        Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public IQueryable<dal.Entities.ShareType> GetDeletedShareTypes([Service]AppDbContext context)
            => context.ShareTypes.Where(x => x.DateDeleted != null);
    }
}
