using System.Linq;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Queries
{
    /// <summary>
    ///     Queries involving <see cname="dal.Entities.Instance" /> objects.
    /// </summary>
    [ExtendObjectType("Query")]
    public class InstanceQueries
    {
        /// <summary>
        ///     Get instances if authorized (Manage Instances)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_INSTANCES)]
        public IQueryable<Instance> GetInstances([ScopedService] AppDbContext context)
        {
            return context.Instances.Where(x => x.DateDeleted == null);
        }

        /// <summary>
        ///     Get instances if authorized (Manage Instances)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_INSTANCES)]
        public IQueryable<Instance> GetDeletedInstances([ScopedService] AppDbContext context)
        {
            return context.Instances.Where(x => x.DateDeleted != null);
        }
    }
}
