using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Instances.Queries;
using JahnDigital.StudentBank.Application.Instances.Queries.GetInstances;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Infrastructure.Persistence;
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
        public InstanceQueries(ISender mediatr) : base(mediatr) { }

        /// <summary>
        ///     Get instances if authorized (Manage Instances)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_INSTANCES)]
        public async Task<IQueryable<Instance>> GetInstancesAsync([ScopedService] AppDbContext context)
        {
            return await _mediatr.Send(new GetInstancesQuery());
        }

        /// <summary>
        ///     Get instances if authorized (Manage Instances)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_INSTANCES)]
        public async Task<IQueryable<Instance>> GetDeletedInstances([ScopedService] AppDbContext context)
        {
            return await _mediatr.Send(new GetInstancesQuery(true));
        }
    }
}
