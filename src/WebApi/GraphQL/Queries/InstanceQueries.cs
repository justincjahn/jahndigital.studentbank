using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Instances.Queries.GetInstances;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.WebApi.GraphQL.Common;
using MediatR;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Queries
{
    /// <summary>
    ///     Queries involving <see cname="dal.Entities.Instance" /> objects.
    /// </summary>
    [ExtendObjectType("Query")]
    public class InstanceQueries : RequestBase
    {
        /// <summary>
        ///     Get instances if authorized (Manage Instances)
        /// </summary>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UsePaging, UseProjection, UseFiltering, UseSorting, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_INSTANCES)]
        public async Task<IQueryable<Instance>> GetInstancesAsync(
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            return await mediatr.Send(new GetInstancesQuery(), cancellationToken);
        }

        /// <summary>
        ///     Get instances if authorized (Manage Instances)
        /// </summary>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UsePaging, UseProjection, UseFiltering, UseSorting, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_INSTANCES)]
        public async Task<IQueryable<Instance>> GetDeletedInstancesAsync(
            [Service] ISender mediatr,
            CancellationToken cancellationToken
         )
        {
            return await mediatr.Send(new GetInstancesQuery(true), cancellationToken);
        }
    }
}
