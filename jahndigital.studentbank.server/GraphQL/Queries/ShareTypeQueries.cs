using System.Linq;
using HotChocolate;
using HotChocolate.Types;
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
            => context.ShareTypes;
    }
}
