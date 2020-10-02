using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.dal.Entities;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    [ExtendObjectType(Name = "Query")]
    public class GroupQueries
    {
        /// <summary>
        /// Get groups
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseSelection]
        public IQueryable<dal.Entities.Group> GetGroups([Service]AppDbContext context)
            => context.Groups;
    }
}
