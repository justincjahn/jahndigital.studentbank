using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application;
using JahnDigital.StudentBank.Application.Instances.Commands.NewInstance;
using JahnDigital.StudentBank.Application.Instances.Commands.UpdateInstance;
using JahnDigital.StudentBank.Application.Instances.Queries.GetInstance;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.WebApi.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Mutations
{
    /// <summary>
    ///     CRUD operations for <see cref="Instance" /> entities.
    /// </summary>
    [ExtendObjectType("Mutation")]
    public class InstanceMutations
    {
        /// <summary>
        ///     Update an <see cref="Instance" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Policy = Privilege.PRIVILEGE_MANAGE_INSTANCES)]
        public async Task<IQueryable<Instance>> UpdateInstanceAsync(
            UpdateInstanceRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new UpdateInstanceCommand(
                input.Id,
                input.Description,
                input.IsActive
            ), cancellationToken);

            return await mediatr.Send(new GetInstanceQuery(input.Id), cancellationToken);
        }

        /// <summary>
        ///     Create an <see cref="Instance" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="configuration"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Policy = Privilege.PRIVILEGE_MANAGE_INSTANCES)]
        public async Task<Instance> NewInstanceAsync(
            NewInstanceRequest input,
            [Service] ISender mediatr,
            [Service] IConfiguration configuration,
            CancellationToken cancellationToken
        )
        {
            var length = configuration.Get<AppConfig>().InviteCodeLength;
            var instanceId = await mediatr.Send(new NewInstanceCommand(input.Description, length), cancellationToken);
            return await (await mediatr.Send(new GetInstanceQuery(instanceId), cancellationToken)).FirstAsync(cancellationToken);
        }

        /// <summary>
        ///     Soft-delete an <see cref="Instance" />.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Policy = Privilege.PRIVILEGE_MANAGE_INSTANCES)]
        public async Task<bool> DeleteInstanceAsync(
            long id,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new UpdateInstanceCommand(id, null, null, true), cancellationToken);
            return true;
        }

        /// <summary>
        ///     Restore a soft-deleted <see cref="Instance" />.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_INSTANCES)]
        public async Task<IQueryable<Instance>> RestoreInstanceAsync(
            long id,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new UpdateInstanceCommand(id, null, null, false), cancellationToken);
            return await mediatr.Send(new GetInstanceQuery(id), cancellationToken);
        }
    }
}
