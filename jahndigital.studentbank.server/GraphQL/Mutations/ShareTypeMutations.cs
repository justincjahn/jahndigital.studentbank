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
    [ExtendObjectType(Name = "Mutation")]
    public class ShareTypeMutations
    {
        /// <summary>
        /// Update a <see cref="dal.Entities.ShareType"/>.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public async Task<IQueryable<dal.Entities.ShareType>> UpdateShareTypeAsync(
            UpdateShareTypeRequest input,
            [Service] AppDbContext context
        ) {
            var shareType = await context.ShareTypes.FindAsync(input.Id)
                ?? throw ErrorFactory.NotFound();
            
            shareType.DividendRate = input.DividendRate ?? shareType.DividendRate;

            if (input.Name != null) {
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
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.ShareTypes.Where(x => x.Id == shareType.Id);
        }

        /// <summary>
        /// Create a new <see cref="dal.Entities.ShareType"/>.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
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
        /// Soft-delete a <see cref="dal.Entities.ShareType"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<bool> DeleteShareTypeAsync(long id, [Service]AppDbContext context)
        {
            var shareType = await context.ShareTypes.FindAsync(id)
                ?? throw ErrorFactory.NotFound();

            var hasShares = await context.Shares.AnyAsync(x => x.ShareTypeId == id && x.DateDeleted != null);
            if (hasShares) throw ErrorFactory.QueryFailed("Cannot delete a share type with active shares.");

            shareType.DateDeleted = DateTime.UtcNow;

            try {
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return true;
        }
    }
}
