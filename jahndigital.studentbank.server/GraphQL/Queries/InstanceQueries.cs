using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    /// <summary>
    /// Instances
    /// </summary>
    [ExtendObjectType(Name = "Query")]
    public class InstanceQueries
    {
        /// <summary>
        /// Get all instances.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseSelection]
        public IQueryable<dal.Entities.Instance> GetInstances([Service]AppDbContext context) =>
            context.Instances;

        /// <summary>
        /// Get a specific instance by ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseSelection]
        public IQueryable<dal.Entities.Instance> GetInstance(long id, [Service]AppDbContext context) =>
            context.Instances.Where(x => x.Id == id);
    }
}
