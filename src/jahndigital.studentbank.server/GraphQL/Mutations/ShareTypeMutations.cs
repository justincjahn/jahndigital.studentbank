using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.Domain.ValueObjects;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using jahndigital.studentbank.server.Models;
using Microsoft.EntityFrameworkCore;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace jahndigital.studentbank.server.GraphQL.Mutations
{
    /// <summary>
    ///     CRUD operations for <see cref="dal.Entities.ShareType" /> entities.
    /// </summary>
    [ExtendObjectType("Mutation")]
    public class ShareTypeMutations
    {
        /// <summary>
        ///     Create a new <see cref="dal.Entities.ShareType" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection,
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public async Task<IQueryable<ShareType>> NewShareTypeAsync(
            NewShareTypeRequest input,
            [ScopedService] AppDbContext context
        )
        {
            if (await context.ShareTypes.AnyAsync(x => x.Name == input.Name))
            {
                throw new ArgumentOutOfRangeException(
                    "Name",
                    $"A Share Type with the name '{input.Name}' already exists."
                );
            }

            ShareType? shareType = new ShareType
            {
                Name = input.Name,
                DividendRate = input.DividendRate,
                WithdrawalLimitCount = input.WithdrawalLimitCount ?? 0,
                WithdrawalLimitPeriod = input.WithdrawalLimitPeriod ?? Period.Monthly,
                WithdrawalLimitShouldFee = input.WithdrawalLimitShouldFee ?? false,
                WithdrawalLimitFee = input.WithdrawalLimitFee ?? Money.FromCurrency(0)
            };

            try
            {
                context.Add(shareType);
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.ShareTypes.Where(x => x.Id == shareType.Id);
        }

        /// <summary>
        ///     Update a <see cref="dal.Entities.ShareType" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection,
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public async Task<IQueryable<ShareType>> UpdateShareTypeAsync(
            UpdateShareTypeRequest input,
            [ScopedService] AppDbContext context
        )
        {
            ShareType? shareType = await context.ShareTypes.FindAsync(input.Id)
                ?? throw ErrorFactory.NotFound();

            shareType.DividendRate = input.DividendRate ?? shareType.DividendRate;
            shareType.WithdrawalLimitCount = input.WithdrawalLimitCount ?? shareType.WithdrawalLimitCount;
            shareType.WithdrawalLimitPeriod = input.WithdrawalLimitPeriod ?? shareType.WithdrawalLimitPeriod;
            shareType.WithdrawalLimitShouldFee = input.WithdrawalLimitShouldFee ?? shareType.WithdrawalLimitShouldFee;
            shareType.WithdrawalLimitFee = input.WithdrawalLimitFee ?? shareType.WithdrawalLimitFee;

            if (input.Name != null && input.Name != shareType.Name)
            {
                if (await context.ShareTypes.Where(x => x.Name == input.Name).AnyAsync())
                {
                    throw new ArgumentOutOfRangeException(
                        "Name",
                        $"A Share Type with the name '{input.Name}' already exists."
                    );
                }

                shareType.Name = input.Name;
            }

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                throw ErrorFactory.QueryFailed(e.InnerException?.Message ?? e.Message);
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.ShareTypes.Where(x => x.Id == shareType.Id);
        }

        /// <summary>
        ///     Link a <see cref="dal.Entities.ShareType" /> to an <see cref="Instance" />.<see langword="abstract" />
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection,
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public async Task<IQueryable<ShareType>> LinkShareTypeAsync(
            LinkShareTypeRequest input,
            [ScopedService] AppDbContext context
        )
        {
            bool hasInstance = await context.Instances.AnyAsync(x => x.Id == input.InstanceId);

            if (!hasInstance)
            {
                throw ErrorFactory.NotFound();
            }

            bool hasShareType = await context.ShareTypes.AnyAsync(x => x.Id == input.ShareTypeId);

            if (!hasShareType)
            {
                throw ErrorFactory.NotFound();
            }

            bool hasLinks = await context.ShareTypeInstances
                .Where(x => x.ShareTypeId == input.ShareTypeId && x.InstanceId == input.InstanceId)
                .AnyAsync();

            if (hasLinks)
            {
                throw ErrorFactory.QueryFailed("Share Type is already linked to the provided instance!");
            }

            ShareTypeInstance? link =
                new ShareTypeInstance { ShareTypeId = input.ShareTypeId, InstanceId = input.InstanceId };

            try
            {
                context.Add(link);
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.ShareTypes.Where(x => x.Id == input.ShareTypeId);
        }

        /// <summary>
        ///     Unlink a <see cref="dal.Entities.ShareType" /> from an <see cref="Instance" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection,
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public async Task<IQueryable<ShareType>> UnlinkShareTypeAsync(
            LinkShareTypeRequest input,
            [ScopedService] AppDbContext context
        )
        {
            bool hasInstance = await context.Instances.AnyAsync(x => x.Id == input.InstanceId);

            if (!hasInstance)
            {
                throw ErrorFactory.NotFound();
            }

            bool hasShareType = await context.ShareTypes.AnyAsync(x => x.Id == input.ShareTypeId);

            if (!hasShareType)
            {
                throw ErrorFactory.NotFound();
            }

            ShareTypeInstance? link = await context.ShareTypeInstances
                .Where(x => x.ShareTypeId == input.ShareTypeId && x.InstanceId == input.InstanceId)
                .FirstOrDefaultAsync();

            if (link == null)
            {
                throw ErrorFactory.QueryFailed("Share Type is already unlinked from the provided instance!");
            }

            bool hasSharesInInstance = await context.Shares
                .Include(x => x.Student)
                .ThenInclude(x => x.Group)
                .Where(x =>
                    x.Student.Group.InstanceId == input.InstanceId
                    && x.ShareTypeId == input.ShareTypeId
                    && x.DateDeleted == null)
                .AnyAsync();

            if (hasSharesInInstance)
            {
                throw ErrorFactory.QueryFailed(
                    "There are still open shares of this share type in the instance you are trying to unlink!"
                );
            }

            try
            {
                context.Remove(link);
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.ShareTypes.Where(x => x.Id == input.ShareTypeId);
        }

        /// <summary>
        ///     Soft-delete a <see cref="dal.Entities.ShareType" />.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)),
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public async Task<bool> DeleteShareTypeAsync(
            long id,
            [ScopedService] AppDbContext context
        )
        {
            ShareType? shareType = await context.ShareTypes.FindAsync(id)
                ?? throw ErrorFactory.NotFound();

            bool hasShares = await context.Shares.AnyAsync(x => x.ShareTypeId == id && x.DateDeleted == null);

            if (hasShares)
            {
                throw ErrorFactory.QueryFailed("Cannot delete a share type with active shares.");
            }

            shareType.DateDeleted = DateTime.UtcNow;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return true;
        }

        /// <summary>
        ///     Restore a soft-deleted <see cref="dal.Entities.ShareType" />.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)),
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public async Task<IQueryable<ShareType>> RestoreShareTypeAsync(
            long id,
            [ScopedService] AppDbContext context
        )
        {
            ShareType? shareType = await context.ShareTypes
                    .Where(x => x.Id == id && x.DateDeleted != null)
                    .SingleOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            shareType.DateDeleted = null;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.ShareTypes.Where(x => x.Id == id);
        }
    }
}
