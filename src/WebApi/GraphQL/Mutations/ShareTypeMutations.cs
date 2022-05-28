using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.ShareTypes.Commands.DeleteShareTypeCommand;
using JahnDigital.StudentBank.Application.ShareTypes.Commands.LinkShareTypeCommand;
using JahnDigital.StudentBank.Application.ShareTypes.Commands.NewShareType;
using JahnDigital.StudentBank.Application.ShareTypes.Commands.RestoreShareTypeCommand;
using JahnDigital.StudentBank.Application.ShareTypes.Commands.UnlinkShareTypeCommand;
using JahnDigital.StudentBank.Application.ShareTypes.Commands.UpdateShareType;
using JahnDigital.StudentBank.Application.ShareTypes.Queries.GetShareType;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.Domain.ValueObjects;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using JahnDigital.StudentBank.WebApi.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Mutations
{
    /// <summary>
    ///     CRUD operations for <see cref="ShareType" /> entities.
    /// </summary>
    [ExtendObjectType("Mutation")]
    public class ShareTypeMutations
    {
        /// <summary>
        ///     Create a new <see cref="ShareType" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public async Task<IQueryable<ShareType>> NewShareTypeAsync(
            NewShareTypeRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            var shareTypeId = await mediatr.Send(new NewShareTypeCommand
            {
                Name = input.Name,
                DividendRate = input.DividendRate,
                WithdrawalLimitCount = input.WithdrawalLimitCount ?? 0,
                WithdrawalLimitPeriod = input.WithdrawalLimitPeriod ?? Period.Monthly,
                WithdrawalLimitShouldFee = input.WithdrawalLimitShouldFee ?? false,
                WithdrawalLimitFee = input.WithdrawalLimitFee ?? Money.Zero
            }, cancellationToken);

            return await mediatr.Send(new GetShareTypeQuery(shareTypeId), cancellationToken);
        }

        /// <summary>
        ///     Update a <see cref="ShareType" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public async Task<IQueryable<ShareType>> UpdateShareTypeAsync(
            UpdateShareTypeRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new UpdateShareTypeCommand
            {
                ShareTypeId = input.Id,
                Name = input.Name,
                DividendRate = input.DividendRate,
                WithdrawalLimitCount = input.WithdrawalLimitCount,
                WithdrawalLimitPeriod = input.WithdrawalLimitPeriod,
                WithdrawalLimitShouldFee = input.WithdrawalLimitShouldFee,
                WithdrawalLimitFee = input.WithdrawalLimitFee
            }, cancellationToken);

            return await mediatr.Send(new GetShareTypeQuery(input.Id), cancellationToken);
        }

        /// <summary>
        ///     Link a <see cref="ShareType" /> to an <see cref="Instance" />.<see langword="abstract" />
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public async Task<IQueryable<ShareType>> LinkShareTypeAsync(
            LinkShareTypeRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new LinkShareTypeCommand(input.ShareTypeId, input.InstanceId), cancellationToken);
            return await mediatr.Send(new GetShareTypeQuery(input.ShareTypeId), cancellationToken);
        }

        /// <summary>
        ///     Unlink a <see cref="ShareType" /> from an <see cref="Instance" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public async Task<IQueryable<ShareType>> UnlinkShareTypeAsync(
            LinkShareTypeRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new UnlinkShareTypeCommand(input.ShareTypeId, input.InstanceId), cancellationToken);
            return await mediatr.Send(new GetShareTypeQuery(input.ShareTypeId), cancellationToken);
        }

        /// <summary>
        ///     Soft-delete a <see cref="ShareType" />.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public async Task<bool> DeleteShareTypeAsync(
            long id,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new DeleteShareTypeCommand(id), cancellationToken);
            return true;
        }

        /// <summary>
        ///     Restore a soft-deleted <see cref="ShareType" />.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public async Task<IQueryable<ShareType>> RestoreShareTypeAsync(
            long id,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new RestoreShareTypeCommand(id), cancellationToken);
            return await mediatr.Send(new GetShareTypeQuery(id), cancellationToken);
        }
    }
}
