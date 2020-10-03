using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    /// <summary>
    /// Queries involving <see cname="dal.Entities.Instance" /> objects.
    /// </summary>
    [ExtendObjectType(Name = "Query")]
    public class InstanceQueries
    {
        /// <summary>
        /// Get instances if authorized (Manage Instances)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseFiltering, UseSorting, UseSelection]
        public IQueryable<dal.Entities.Instance> GetInstances([Service]AppDbContext context) =>
            context.Instances;
    }
}
