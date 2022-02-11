using System.Linq;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    /// <summary>
    ///     Operations around querying groups.
    /// </summary>
    [ExtendObjectType("Query")]
    public class GroupQueries
    {
        /// <summary>
        ///     Get groups if authorized (Manage Groups).
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_GROUPS)]
        public IQueryable<Group> GetGroups([ScopedService] AppDbContext context)
        {
            return context.Groups.Where(x => x.DateDeleted == null);
        }

        /// <summary>
        ///     Get deleted groups if authorized (Manage Groups).
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_GROUPS)]
        public IQueryable<Group> GetDeletedGroups([ScopedService] AppDbContext context)
        {
            return context.Groups.Where(x => x.DateDeleted != null);
        }
    }
}
