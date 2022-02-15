using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Groups.Queries;
using JahnDigital.StudentBank.Application.Groups.Queries.GetGroups;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using JahnDigital.StudentBank.WebApi.GraphQL.Common;
using MediatR;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Queries
{
    /// <summary>
    ///     Operations around querying groups.
    /// </summary>
    [ExtendObjectType("Query")]
    public class GroupQueries : RequestBase
    {
        public GroupQueries(ISender mediatr) : base(mediatr) { }

        /// <summary>
        ///     Get groups if authorized (Manage Groups).
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_GROUPS)]
        public async Task<IQueryable<Group>> GetGroupsAsync([ScopedService] AppDbContext context)
        {
            return await _mediatr.Send(new GetGroupsQuery());
        }

        /// <summary>
        ///     Get deleted groups if authorized (Manage Groups).
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_GROUPS)]
        public async Task<IQueryable<Group>> GetDeletedGroups([ScopedService] AppDbContext context)
        {
            return await _mediatr.Send(new GetGroupsQuery(true));
        }
    }
}
