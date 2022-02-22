using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Groups.Commands.NewGroup;
using JahnDigital.StudentBank.Application.Groups.Commands.UpdateGroup;
using JahnDigital.StudentBank.Application.Groups.Queries.GetGroup;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.WebApi.Models;
using MediatR;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Mutations
{
    /// <summary>
    ///     CRUD operations for <see cref="Group" /> entities..
    /// </summary>
    [ExtendObjectType("Mutation")]
    public class GroupMutations
    {
        /// <summary>
        ///     Update a <see cref="Group" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_GROUPS)]
        public async Task<IQueryable<Group>> UpdateGroupAsync(
            UpdateGroupRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new UpdateGroupCommand(
                input.Id,
                input.InstanceId,
                input.Name
            ), cancellationToken);

            return await mediatr.Send(new GetGroupQuery(input.Id), cancellationToken);
        }

        /// <summary>
        ///     Create a new <see cref="Group" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_GROUPS)]
        public async Task<IQueryable<Group>> NewGroupAsync(
            NewGroupRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            var id = await mediatr.Send(new NewGroupCommand(input.InstanceId, input.Name), cancellationToken);
            return await mediatr.Send(new GetGroupQuery(id), cancellationToken);
        }

        /// <summary>
        ///     Delete a <see cref="Group" />.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Policy = Privilege.PRIVILEGE_MANAGE_GROUPS)]
        public async Task<bool> DeleteGroupAsync(
            long id,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new UpdateGroupCommand(id, null, null, true), cancellationToken);
            return true;
        }

        /// <summary>
        ///     Restore a soft-deleted <see cref="Group" />.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_GROUPS)]
        public async Task<IQueryable<Group>> RestoreGroupAsync(
            long id,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new UpdateGroupCommand(id, null, null, false), cancellationToken);
            return await mediatr.Send(new GetGroupQuery(id), cancellationToken);
        }
    }
}
