using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Groups.DTOs;
using JahnDigital.StudentBank.Application.Groups.Queries.GetGroups;
using JahnDigital.StudentBank.Application.Groups.Queries.GetGroupStatistics;
using JahnDigital.StudentBank.Domain.Entities;
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
        /// <summary>
        ///     Get groups if authorized (Manage Groups).
        /// </summary>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UsePaging, UseProjection, UseFiltering, UseSorting, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_GROUPS)]
        public async Task<IQueryable<Group>> GetGroupsAsync(
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            return await mediatr.Send(new GetGroupsQuery(), cancellationToken);
        }

        /// <summary>
        ///     Get deleted groups if authorized (Manage Groups).
        /// </summary>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UsePaging, UseProjection, UseFiltering, UseSorting, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_GROUPS)]
        public async Task<IQueryable<Group>> GetDeletedGroupsAsync(
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            return await mediatr.Send(new GetGroupsQuery(null, true), cancellationToken);
        }

        /// <summary>
        ///     Gets totals for the groups, if authorized (Manage Groups).
        /// </summary>
        /// <param name="groupId">One or more Group Ids to fetch statistics for.</param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Policy = Privilege.PRIVILEGE_MANAGE_GROUPS)]
        public async Task<IEnumerable<GroupStatisticsResponse>> GetGroupStatistics(
            long[] groupId,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            return await mediatr.Send(new GetGroupStatisticsQuery(groupId), cancellationToken);
        }
    }
}
