using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.services.Exceptions;
using jahndigital.studentbank.services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace jahndigital.studentbank.services
{
    public class ShareTypeService : IShareTypeService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        public ShareTypeService(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
        }


        /// <summary>
        ///     Reset the withdrawal limit of a given ShareType ID.
        /// </summary>
        /// <param name="shareTypeId"></param>
        /// <returns></returns>
        public async Task<bool> ResetWithdrawalLimit(long shareTypeId)
        {
            await using AppDbContext? context = _factory.CreateDbContext();

            ShareType? shareType = await context.ShareTypes
                    .Where(x => x.Id == shareTypeId)
                    .SingleOrDefaultAsync()
                ?? throw new ShareTypeNotFoundException(shareTypeId);

            IDbContextTransaction? transaction = await context.Database.BeginTransactionAsync();
            IQueryable<Share>? query = context.Shares.Where(x => x.ShareTypeId == shareTypeId);
            int count = await query.CountAsync();

            for (int i = 0; i < count; i += 100)
            {
                List<Share>? shares = await query.Skip(i).Take(100).ToListAsync();

                foreach (Share? share in shares)
                {
                    share.LimitedWithdrawalCount = 0;
                }

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    throw new DatabaseException(e.Message);
                }
            }

            try
            {
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                throw new DatabaseException(e.Message);
            }

            shareType.WithdrawalLimitLastReset = DateTime.UtcNow;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new DatabaseException(e.Message);
            }

            return true;
        }
    }
}
