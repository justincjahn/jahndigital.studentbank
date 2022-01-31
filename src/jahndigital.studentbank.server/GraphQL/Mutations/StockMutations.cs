using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Execution;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.server.Models;
using jahndigital.studentbank.utils;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.server.GraphQL.Mutations
{
    /// <summary>
    ///     CRUD operations for <see cref="dal.Entities.Stock" /> entities.
    /// </summary>
    [ExtendObjectType("Mutation")]
    public class StockMutations
    {
        /// <summary>
        ///     Create a new stock.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection,
         Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public async Task<IQueryable<Stock>> NewStockAsync(
            NewStockRequest input,
            [ScopedService] AppDbContext context
        )
        {
            bool hasStock = await context.Stocks.AnyAsync(x => x.Symbol == input.Symbol);

            if (hasStock)
            {
                throw ErrorFactory.QueryFailed(
                    $"A stock already exists with symbol '{input.Symbol}'!  Was it deleted?"
                );
            }

            Stock? stock = new Stock
            {
                Symbol = input.Symbol,
                Name = input.Name,
                CurrentValue = input.CurrentValue,
                TotalShares = input.TotalShares,
                AvailableShares = input.TotalShares
            };

            stock.History.Add(new StockHistory { Stock = stock, Value = input.CurrentValue });

            try
            {
                context.Add(stock);
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Stocks.Where(x => x.Id == stock.Id);
        }

        /// <summary>
        ///     Update a <see cref="dal.Entities.Stock" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection,
         Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public async Task<IQueryable<Stock>> UpdateStockAsync(
            UpdateStockRequest input,
            [ScopedService] AppDbContext context
        )
        {
            Stock? stock = await context.Stocks.FindAsync(input.Id)
                ?? throw ErrorFactory.NotFound();

            if (input.Name != null && stock.Name != input.Name)
            {
                bool stockExists = await context.Stocks
                    .Where(x => x.Name == input.Name && x.Id != input.Id)
                    .AnyAsync();

                if (stockExists)
                {
                    throw ErrorFactory.QueryFailed($"A stock named {input.Name} already exists.");
                }

                stock.Name = input.Name;
            }

            if (input.Symbol != null && input.Symbol.ToUpper() != stock.Symbol)
            {
                string? symbol = input.Symbol.ToUpper();
                bool symbolExists = await context.Stocks
                    .Where(x => x.Symbol == symbol && x.Id != input.Id)
                    .AnyAsync();

                if (symbolExists)
                {
                    throw ErrorFactory.QueryFailed($"A stock with the symbol {symbol} already exists.");
                }

                stock.Symbol = symbol;
            }

            if (input.TotalShares != null && stock.TotalShares != input.TotalShares)
            {
                // TODO Check for sold shares, force a buyback instead of hard-stopping reducing shares.
                if (input.TotalShares < stock.TotalShares)
                {
                    throw ErrorFactory.QueryFailed(
                        $"Total shares for {stock.Name} cannot be less than the current amount of {stock.TotalShares}."
                    );
                }

                stock.TotalShares = input.TotalShares.Value;
            }

            if (input.CurrentValue is not null && stock.CurrentValue != input.CurrentValue)
            {
                context.StockHistory.Add(new StockHistory { Stock = stock, Value = input.CurrentValue });

                stock.CurrentValue = input.CurrentValue;
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

            return context.Stocks.Where(x => x.Id == stock.Id);
        }

        /// <summary>
        /// Purge stock history for a given stock to a given date.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="QueryException"></exception>
        [UseDbContext(typeof(AppDbContext)),
         Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public async Task<IEnumerable<StockHistory>> PurgeStockHistoryAsync(
            PurgeStockRequest input,
            [ScopedService] AppDbContext context
        )
        {
            Stock? stock = await context.Stocks.FindAsync(input.StockId)
                ?? throw ErrorFactory.NotFound();

            List<StockHistory>? history = await context.StockHistory
                .Where(x => x.StockId == stock.Id && x.DateChanged < input.Date)
                .ToListAsync();

            context.StockHistory.RemoveRange(history);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return history;
        }

        /// <summary>
        ///     Link a <see cref="dal.Entities.Stock" /> to an <see cref="dal.Entities.Instance" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection,
         Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public async Task<IQueryable<Stock>> LinkStockAsync(
            LinkStockRequest input,
            [ScopedService] AppDbContext context
        )
        {
            bool hasInstance = await context.Instances.AnyAsync(x => x.Id == input.InstanceId);

            if (!hasInstance)
            {
                throw ErrorFactory.NotFound();
            }

            bool hasStock = await context.Stocks.AnyAsync(x => x.Id == input.StockId);

            if (!hasStock)
            {
                throw ErrorFactory.NotFound();
            }

            bool hasLinks = await context.StockInstances
                .Where(x => x.StockId == input.StockId && x.InstanceId == input.InstanceId)
                .AnyAsync();

            if (hasLinks)
            {
                throw ErrorFactory.QueryFailed("Stock is already linked to the provided instance!");
            }

            StockInstance? link = new StockInstance { StockId = input.StockId, InstanceId = input.InstanceId };

            try
            {
                context.Add(link);
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Stocks.Where(x => x.Id == input.StockId);
        }

        /// <summary>
        ///     Unlink a <see cref="dal.Entities.Stock" /> from an <see cref="dal.Entities.Instance" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection,
         Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public async Task<IQueryable<Stock>> UnlinkStockAsync(
            LinkStockRequest input,
            [ScopedService] AppDbContext context
        )
        {
            bool hasInstance = await context.Instances.AnyAsync(x => x.Id == input.InstanceId);

            if (!hasInstance)
            {
                throw ErrorFactory.NotFound();
            }

            bool hasStock = await context.Stocks.AnyAsync(x => x.Id == input.StockId);

            if (!hasStock)
            {
                throw ErrorFactory.NotFound();
            }

            StockInstance? link = await context.StockInstances
                .Where(x => x.StockId == input.StockId && x.InstanceId == input.InstanceId)
                .FirstOrDefaultAsync();

            if (link == null)
            {
                throw ErrorFactory.QueryFailed("Stock is already unlinked to the provided instance!");
            }

            // Determine if the students in the instance being unlinked still have shares.
            bool hasIssuedShares = await context.StudentStocks
                .Include(x => x.Student)
                .ThenInclude(x => x.Group)
                .Where(x =>
                    x.Student.Group.InstanceId == input.InstanceId
                    && x.StockId == input.StockId
                    && x.SharesOwned > 0
                )
                .AnyAsync();

            if (hasIssuedShares)
            {
                throw ErrorFactory.QueryFailed(
                    "There are still students in this instance who own shares of this stock.  Please buy them out first!"
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

            return context.Stocks.Where(x => x.Id == input.StockId);
        }

        /// <summary>
        ///     Soft-delete a stock.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)),
         Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public async Task<bool> DeleteStockAsync(
            long id,
            [ScopedService] AppDbContext context
        )
        {
            Stock? stock = await context.Stocks.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw ErrorFactory.NotFound();

            bool hasPurchases = await context.StudentStocks.AnyAsync(x => x.SharesOwned > 0 && x.StockId == stock.Id);

            if (hasPurchases)
            {
                throw ErrorFactory.QueryFailed(
                    "There are still students who own shares of this stock.  Please buy them out first!"
                );
            }

            bool hasLinks = await context.StockInstances.AnyAsync(x => x.StockId == stock.Id);

            if (hasLinks)
            {
                throw ErrorFactory.QueryFailed("Cannot delete a stock that's still linked to an instance!");
            }

            stock.DateDeleted = DateTime.UtcNow;

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
        ///     Restore a deleted stock.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)),
         UseProjection, Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public async Task<IQueryable<Stock>> RestoreStockAsync(
            long id,
            [ScopedService] AppDbContext context
        )
        {
            Stock? stock = await context.Stocks.SingleOrDefaultAsync(x => x.Id == id && x.DateDeleted != null)
                ?? throw ErrorFactory.NotFound();

            stock.DateDeleted = null;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Stocks.Where(x => x.Id == stock.Id);
        }
    }
}
