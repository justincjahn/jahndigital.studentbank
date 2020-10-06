using System.Linq;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using jahndigital.studentbank.dal.Contexts;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    [ExtendObjectType(Name = "Query")]
    public class ShareQueries
    {
        /// <summary>
        /// Get shares by student ID.
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseSelection,
        Authorize(Policy = Constants.AuthPolicy.DataOwner + "<" + Constants.Privilege.PRIVILEGE_MANAGE_SHARES + ">")]
        public IQueryable<dal.Entities.Share> GetShares(long studentId, [Service]AppDbContext context) =>
            context.Shares.Where(x => x.StudentId == studentId && x.DateDeleted == null);
        
        /// <summary>
        /// Get deleted shares (if authorized).
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UsePaging, UseFiltering, UseSorting, UseSelection,
        Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_SHARES)]
        public IQueryable<dal.Entities.Share> GetDeletedShares([Service]AppDbContext context) =>
            context.Shares.Where(x => x.DateDeleted != null);
    }
}
