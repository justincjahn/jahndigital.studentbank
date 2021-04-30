using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.server.Models;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.server.GraphQL.Mutations
{
    /// <summary>
    /// CRUD operations for <see cref="dal.Entities.ShareType"/> entities.
    /// </summary>
    [ExtendObjectType(Name = "Mutation")]
    public class ShareTypeMutations
    {
        /// <summary>
        /// Create a new <see cref="dal.Entities.ShareType"/>.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseSelection, Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public async Task<IQueryable<dal.Entities.ShareType>> NewShareTypeAsync(
            NewShareTypeRequest input,
            [Service] AppDbContext context
        ) {
            if (await context.ShareTypes.AnyAsync(x => x.Name == input.Name)) {
                throw new ArgumentOutOfRangeException(
                    "Name",
                    $"A Share Type with the name '{input.Name}' already exists."
                );
            }

            var shareType = new dal.Entities.ShareType {
                Name = input.Name,
                DividendRate = input.DividendRate,
            };

            try {
                context.Add(shareType);
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.ShareTypes.Where(x => x.Id == shareType.Id);
        }

        /// <summary>
        /// Update a <see cref="dal.Entities.ShareType"/>.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseSelection, Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public async Task<IQueryable<dal.Entities.ShareType>> UpdateShareTypeAsync(
            UpdateShareTypeRequest input,
            [Service] AppDbContext context
        ) {
            var shareType = await context.ShareTypes.FindAsync(input.Id)
                ?? throw ErrorFactory.NotFound();
            
            shareType.DividendRate = input.DividendRate ?? shareType.DividendRate;

            if (input.Name != null && input.Name != shareType.Name) {
                if (await context.ShareTypes.Where(x => x.Name == input.Name).AnyAsync()) {
                    throw new ArgumentOutOfRangeException(
                        "Name",
                        $"A Share Type with the name '{input.Name}' already exists."
                    );
                }

                shareType.Name = input.Name;
            }

            try {
                await context.SaveChangesAsync();
            } catch (DbUpdateException e) {
                throw ErrorFactory.QueryFailed(e.InnerException?.Message ?? e.Message);
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.ShareTypes.Where(x => x.Id == shareType.Id);
        }

        /// <summary>
        /// Link a <see cref="dal.Entities.ShareType"/> to an <see cref="dal.Entities.Instance"/>.<see langword="abstract"/>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseSelection, Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public async Task<IQueryable<dal.Entities.ShareType>> LinkShareTypeAsync(
            LinkShareTypeRequest input,
            [Service] AppDbContext context
        ) {
            var hasInstance = await context.Instances.AnyAsync(x => x.Id == input.InstanceId);
            if (!hasInstance) throw ErrorFactory.NotFound();

            var hasShareType = await context.ShareTypes.AnyAsync(x => x.Id == input.ShareTypeId);
            if (!hasShareType) throw ErrorFactory.NotFound();

            var hasLinks = await context.ShareTypeInstances
                .Where(x => x.ShareTypeId == input.ShareTypeId && x.InstanceId == input.InstanceId)
                .AnyAsync();

            if (hasLinks) throw ErrorFactory.QueryFailed("Share Type is already linked to the provided instance!");

            var link = new dal.Entities.ShareTypeInstance {
                ShareTypeId = input.ShareTypeId,
                InstanceId = input.InstanceId
            };

            try {
                context.Add(link);
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.ShareTypes.Where(x => x.Id == input.ShareTypeId);
        }

        /// <summary>
        /// Unlink a <see cref="dal.Entities.ShareType"/> from an <see cref="dal.Entities.Instance"/>.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseSelection, Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public async Task<IQueryable<dal.Entities.ShareType>> UnlinkShareTypeAsync(
            LinkShareTypeRequest input,
            [Service] AppDbContext context
        ) {
            var hasInstance = await context.Instances.AnyAsync(x => x.Id == input.InstanceId);
            if (!hasInstance) throw ErrorFactory.NotFound();

            var hasShareType = await context.ShareTypes.AnyAsync(x => x.Id == input.ShareTypeId);
            if (!hasShareType) throw ErrorFactory.NotFound();

            var link = await context.ShareTypeInstances
                .Where(x => x.ShareTypeId == input.ShareTypeId && x.InstanceId == input.InstanceId)
                .FirstOrDefaultAsync();

            if (link == null) throw ErrorFactory.QueryFailed("Share Type is already unlinked from the provided instance!");

            var hasSharesInInstance = await context.Shares
                .Include(x => x.Student)
                    .ThenInclude(x => x.Group)
                .Where(x =>
                    x.Student.Group.InstanceId == input.InstanceId
                    && x.ShareTypeId == input.ShareTypeId
                    && x.DateDeleted == null)
                .AnyAsync();

            if (hasSharesInInstance) {
                throw ErrorFactory.QueryFailed(
                    "There are still open shares of this share type in the instance you are trying to unlink!"
                );
            }

            try {
                context.Remove(link);
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.ShareTypes.Where(x => x.Id == input.ShareTypeId);
        }

        /// <summary>
        /// Soft-delete a <see cref="dal.Entities.ShareType"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public async Task<bool> DeleteShareTypeAsync(long id, [Service]AppDbContext context)
        {
            var shareType = await context.ShareTypes.FindAsync(id)
                ?? throw ErrorFactory.NotFound();

            var hasShares = await context.Shares.AnyAsync(x => x.ShareTypeId == id && x.DateDeleted == null);
            if (hasShares) throw ErrorFactory.QueryFailed("Cannot delete a share type with active shares.");

            shareType.DateDeleted = DateTime.UtcNow;

            try {
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return true;
        }

        /// <summary>
        /// Restore a soft-deleted <see cref="dal.Entities.ShareType"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public async Task<bool> RestoreShareTypeAsync(long id, [Service]AppDbContext context)
        {
            var shareType = await context.ShareTypes
                .Where(x => x.Id == id && x.DateDeleted != null)
                .SingleOrDefaultAsync()
            ?? throw ErrorFactory.NotFound();

            shareType.DateDeleted = null;

            try {
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return true;
        }
    }
}
