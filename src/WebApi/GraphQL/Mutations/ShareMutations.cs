using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Shares.Commands.DeleteShare;
using JahnDigital.StudentBank.Application.Shares.Commands.NewShare;
using JahnDigital.StudentBank.Application.Shares.Commands.RestoreShare;
using JahnDigital.StudentBank.Application.Shares.Commands.UpdateShare;
using JahnDigital.StudentBank.Application.Shares.Queries.GetShare;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.ValueObjects;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using JahnDigital.StudentBank.WebApi.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Mutations
{
    /// <summary>
    ///     CRUD operations for <see cref="Share" /> entities.
    /// </summary>
    [ExtendObjectType("Mutation")]
    public class ShareMutations
    {
        /// <summary>
        ///     Create a new <see cref="Share" /> .
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARES)]
        public async Task<IQueryable<Share>> NewShareAsync(
            NewShareRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            var shareId = await mediatr.Send(new NewShareCommand(input.StudentId, input.ShareTypeId, Money.Zero), cancellationToken);
            return await mediatr.Send(new GetShareQuery(shareId), cancellationToken);
        }

        /// <summary>
        ///     Update a <see cref="Share" /> .
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARES)]
        public async Task<IQueryable<Share>> UpdateShareAsync(
            UpdateShareRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new UpdateShareCommand(input.Id, input.ShareTypeId), cancellationToken);
            return await mediatr.Send(new GetShareQuery(input.Id), cancellationToken);
        }

        /// <summary>
        ///     Soft-delete a <see cref="Share" /> .
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARES)]
        public async Task<bool> DeleteShareAsync(long id, [Service] ISender mediatr, CancellationToken cancellationToken)
        {
            await mediatr.Send(new DeleteShareCommand(id), cancellationToken);
            return true;
        }

        /// <summary>
        ///     Restore a soft-deleted <see cref="Share" />.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARES)]
        public async Task<IQueryable<Share>> RestoreShareAsync(long id, [Service] ISender mediatr, CancellationToken cancellationToken)
        {
            await mediatr.Send(new RestoreShareCommand(id), cancellationToken);
            return await mediatr.Send(new GetShareQuery(id), cancellationToken);
        }
    }
}
