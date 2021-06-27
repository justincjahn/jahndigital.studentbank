using System;
using System.Linq;
using System.Threading.Tasks;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.services.Exceptions;
using jahndigital.studentbank.services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.services
{
    public class ShareTypeService : IShareTypeService
    {
        private readonly AppDbContext _context;

        public ShareTypeService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        ///     Reset the withdrawal limit of a given ShareType ID.
        /// </summary>
        /// <param name="shareTypeId"></param>
        /// <returns></returns>
        public async Task<bool> ResetWithdrawalLimit(long shareTypeId)
        {
            var shareType = await _context.ShareTypes
                    .Where(x => x.Id == shareTypeId)
                    .SingleOrDefaultAsync()
                ?? throw new ShareTypeNotFoundException(shareTypeId);

            var transaction = await _context.Database.BeginTransactionAsync();
            var query = _context.Shares.Where(x => x.ShareTypeId == shareTypeId);
            var count = await query.CountAsync();

            for (var i = 0; i < count; i += 100) {
                var shares = await query.Skip(i).Take(100).ToListAsync();

                foreach (var share in shares) {
                    share.LimitedWithdrawalCount = 0;
                }

                try {
                    await _context.SaveChangesAsync();
                } catch (Exception e) {
                    throw new DatabaseException(e.Message);
                }
            }

            try {
                await transaction.CommitAsync();
            } catch (Exception e) {
                throw new DatabaseException(e.Message);
            }

            shareType.WithdrawalLimitLastReset = DateTime.UtcNow;

            try {
                await _context.SaveChangesAsync();
            } catch (Exception e) {
                throw new DatabaseException(e.Message);
            }

            return true;
        }
    }
}