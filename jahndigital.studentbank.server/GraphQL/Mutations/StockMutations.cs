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
    /// CRUD operations for <see cref="dal.Entities.Stock"/> entities.
    /// </summary>
    [ExtendObjectType(Name = "Mutation")]
    public class StockMutations
    {
        /// <summary>
        /// Create a new stock.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseSelection, Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public async Task<IQueryable<dal.Entities.Stock>> NewStockAsync (
            NewStockRequest input,
            [Service]AppDbContext context
        ) {
            var hasStock = await context.Stocks.AnyAsync(x => x.Symbol == input.Symbol);

            if (hasStock) {
                throw ErrorFactory.QueryFailed(
                    $"A stock already exists with symbol '{input.Symbol}'!  Was it deleted?"
                );
            }

            var stock = new dal.Entities.Stock {
                Symbol = input.Symbol,
                Name = input.Name,
                CurrentValue = input.CurrentValue,
                TotalShares = input.TotalShares,
                AvailableShares = input.TotalShares
            };

            stock.History.Add(new dal.Entities.StockHistory {
                Stock = stock,
                Value = input.CurrentValue
            });

            try {
                context.Add(stock);
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Stocks.Where(x => x.Id == stock.Id);
        }

        /// <summary>
        /// Link a <see cref="dal.Entities.Stock"/> to an <see cref="dal.Entities.Instance"/>.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public async Task<bool> LinkStockAsync(LinkStockRequest input, [Service]AppDbContext context)
        {
            var hasInstance = await context.Instances.AnyAsync(x => x.Id == input.InstanceId);
            if (!hasInstance) throw ErrorFactory.NotFound();

            var hasStock = await context.Stocks.AnyAsync(x => x.Id == input.StockId);
            if (!hasStock) throw ErrorFactory.NotFound();

            var hasLinks = await context.StockInstances
                .Where(x => x.StockId == input.StockId && x.InstanceId == input.InstanceId)
                .AnyAsync();
            
            if (hasLinks) throw ErrorFactory.QueryFailed("Stock is already linked to the provided instance!");

            var link = new dal.Entities.StockInstance {
                StockId = input.StockId,
                InstanceId = input.InstanceId
            };

            try {
                context.Add(link);
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return true;
        }

        /// <summary>
        /// Unlink a <see cref="dal.Entities.Stock"/> from an <see cref="dal.Entities.Instance"/>.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public async Task<bool> UnlinkStockAsync(LinkStockRequest input, [Service]AppDbContext context)
        {
            var hasInstance = await context.Instances.AnyAsync(x => x.Id == input.InstanceId);
            if (!hasInstance) throw ErrorFactory.NotFound();

            var hasStock = await context.Stocks.AnyAsync(x => x.Id == input.StockId);
            if (!hasStock) throw ErrorFactory.NotFound();

            var link = await context.StockInstances
                .Where(x => x.StockId == input.StockId && x.InstanceId == input.InstanceId)
                .FirstOrDefaultAsync();
            
            if (link == null) throw ErrorFactory.QueryFailed("Stock is already unlinked to the provided instance!");

            // Determine if the students in the instance being unlinked still have shares.
            var hasIssuedShares = await context.StockInstances
                .Include(x => x.Stock)
                    .ThenInclude(x => x.StudentStock)
                .Where(x =>
                    x.StockId == input.StockId
                    && x.InstanceId == input.InstanceId
                    && x.Stock.StudentStock.Any(x => x.SharesOwned > 0))
                .AnyAsync();

            if (hasIssuedShares) {
                throw ErrorFactory.QueryFailed(
                    "There are still students in this instance who own shares of this stock.  Please buy them out first!"
                );
            }

            try {
                context.Remove(link);
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return true;
        }

        /// <summary>
        /// Soft-delete a stock.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public async Task<bool> DeleteStockAsync(long id, [Service]AppDbContext context)
        {
            var stock = await context.Stocks.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw ErrorFactory.NotFound();

            var hasPurchases = await context.StudentStocks.AnyAsync(x => x.SharesOwned > 0 && x.StockId == stock.Id);
            if (hasPurchases) {
                throw ErrorFactory.QueryFailed(
                    "There are still students who own shares of this stock.  Please buy them out first!"
                );
            }
            
            var hasLinks = await context.StockInstances.AnyAsync(x => x.StockId == stock.Id);
            if (hasLinks) throw ErrorFactory.QueryFailed("Cannot delete a stock that's still linked to an instance!");

            stock.DateDeleted = DateTime.UtcNow;

            try {
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return true;
        }

        /// <summary>
        /// Restore a deleted stock.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseSelection, Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public async Task<IQueryable<dal.Entities.Stock>> RestoreStockAsync(long id, [Service]AppDbContext context)
        {
            var stock = await context.Stocks.SingleOrDefaultAsync(x => x.Id == id && x.DateDeleted != null)
                ?? throw ErrorFactory.NotFound();
            
            stock.DateDeleted = null;

            try {
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Stocks.Where(x => x.Id == stock.Id);
        }
    }
}
