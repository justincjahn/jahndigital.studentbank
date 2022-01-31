using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.server.Models;
using jahndigital.studentbank.utils;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.server.GraphQL.Mutations
{
    /// <summary>
    ///     CRUD operations for <see cref="dal.Entities.Share" /> entities.
    /// </summary>
    [ExtendObjectType("Mutation")]
    public class ShareMutations
    {
        /// <summary>
        ///     Create a new <see cref="dal.Entities.Share" /> .
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection,
         Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_SHARES)]
        public async Task<IQueryable<Share>> NewShareAsync(
            NewShareRequest input,
            [ScopedService] AppDbContext context
        )
        {
            Student? student = await context.Students
                    .Include(x => x.Group)
                    .ThenInclude(x => x.Instance)
                    .ThenInclude(x => x.ShareTypeInstances)
                    .Where(x => x.Id == input.StudentId)
                    .FirstOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            // Verify the student's instance has been assigned the Share Type ID
            ShareTypeInstance? hasShareType = student.Group.Instance.ShareTypeInstances
                    .SingleOrDefault(x => x.ShareTypeId == input.ShareTypeId)
                ?? throw ErrorFactory.NotFound();

            Share? share = new Share
            {
                StudentId = input.StudentId,
                ShareTypeId = input.ShareTypeId,
                DateLastActive = DateTime.UtcNow,
                Balance = Money.FromCurrency(0.0m)
            };

            try
            {
                context.Add(share);
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Shares.Where(x => x.Id == share.Id);
        }

        /// <summary>
        ///     Update a <see cref="dal.Entities.Share" /> .
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection,
         Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_SHARES)]
        public async Task<IQueryable<Share>> UpdateShareAsync(
            UpdateShareRequest input,
            [ScopedService] AppDbContext context
        )
        {
            Share? share = await context.Shares.Where(x => x.Id == input.Id).FirstOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            Student? student = await context.Students
                    .Include(x => x.Group)
                    .ThenInclude(x => x.Instance)
                    .ThenInclude(x => x.ShareTypeInstances)
                    .Where(x =>
                        x.Group.Instance.ShareTypeInstances
                            .Any(x => x.ShareTypeId == input.ShareTypeId)
                        && x.Id == share.StudentId)
                    .FirstOrDefaultAsync()
                ?? throw ErrorFactory
                    .QueryFailed(
                        $"Student #{share.StudentId} does not have access to Share Type {input.ShareTypeId} or it doesn't exist.");

            share.ShareTypeId = input.ShareTypeId;

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

            return context.Shares.Where(x => x.Id == share.Id);
        }

        /// <summary>
        ///     Soft-delete a <see cref="dal.Entities.Share" /> .
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_SHARES)]
        public async Task<bool> DeleteShareAsync(long id, [Service] AppDbContext context)
        {
            Share? share = await context.Shares
                    .Where(x => x.Id == id)
                    .FirstOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            if (share.Balance != Money.FromCurrency(0.0m))
            {
                throw ErrorFactory.QueryFailed(
                    "Share must be zero-balance before being deleted!"
                );
            }

            share.DateDeleted = DateTime.UtcNow;

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
        ///     Restore a soft-deleted <see cref="dal.Entities.Share" />.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection,
         Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_SHARES)]
        public async Task<IQueryable<Share>> RestoreShareAsync(long id, [ScopedService] AppDbContext context)
        {
            Share? share = await context.Shares
                    .Where(x => x.Id == id && x.DateDeleted != null)
                    .SingleOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            share.DateDeleted = null;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Shares.Where(x => x.Id == id);
        }
    }
}
