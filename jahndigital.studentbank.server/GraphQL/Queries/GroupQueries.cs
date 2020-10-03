using System.Linq;
using HotChocolate;
using HotChocolate.Types;
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
        [UseSelection, UseFiltering]
        public IQueryable<dal.Entities.Group> GetGroups([Service]AppDbContext context)
            => context.Groups;
    }
}
