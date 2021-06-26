using System.Linq;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.utils;

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
        [UsePaging, UseSelection, UseSorting, UseFiltering,
        Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_INSTANCES)]
        public IQueryable<dal.Entities.Instance> GetInstances([Service]AppDbContext context) =>
            context.Instances.Where(x => x.DateDeleted == null);

        /// <summary>
        /// Get instances if authorized (Manage Instances)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UsePaging, UseSelection, UseSorting, UseFiltering,
        Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_INSTANCES)]
        public IQueryable<dal.Entities.Instance> GetDeletedInstances([Service]AppDbContext context) =>
            context.Instances.Where(x => x.DateDeleted != null);
    }
}
