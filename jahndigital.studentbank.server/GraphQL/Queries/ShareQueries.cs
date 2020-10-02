using System.Linq;
using HotChocolate;
using HotChocolate.Types;
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
        [UseSelection]
        public IQueryable<dal.Entities.Share> GetShares(long studentId, [Service]AppDbContext context) =>
            context.Shares.Where(x => x.StudentId == studentId);
    }
}
