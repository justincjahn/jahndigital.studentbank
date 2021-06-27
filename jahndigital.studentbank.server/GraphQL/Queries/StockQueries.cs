using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.utils;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    //[ExtendObjectType(Name = "Query")]
    public class StockQueries
    {
        /// <summary>
        ///     Get a list of stocks available to the user.
        /// </summary>
        /// <param name="instances">One or more instances to use when filtering.</param>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UsePaging, UseSelection, UseSorting, UseFiltering, Authorize]
        public async Task<IQueryable<Stock>> GetStocksAsync(
            IEnumerable<long>? instances,
            [Service] AppDbContext context,
            [Service] IResolverContext resolverContext
        )
        {
            var userType = resolverContext.GetUserType() ?? throw ErrorFactory.Unauthorized();
            var userId = resolverContext.GetUserId() ?? throw ErrorFactory.Unauthorized();
            resolverContext.SetUser(userId, userType);

            if (userType == Constants.UserType.User) {
                var auth = await resolverContext.AuthorizeAsync(Constants.Privilege.ManageStocks.Name);

                if (!auth.Succeeded) {
                    throw ErrorFactory.Unauthorized();
                }

                var stocks = context.Stocks.Where(x => x.DateDeleted == null);

                if (instances != null) {
                    stocks = stocks.Where(x => x.StockInstances.Any(x => instances.Contains(x.InstanceId)));
                }

                return stocks;
            }

            // Fetch the stock IDs the user has access to
            var availableStocks = await context.Students
                    .Include(x => x.Group)
                    .ThenInclude(x => x.Instance)
                    .ThenInclude(x => x.StockInstances)
                    .Where(x => x.Id == userId)
                    .FirstOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            var stockIds = availableStocks.Group.Instance.StockInstances.Select(x => x.StockId);

            return context.Stocks.Where(x => x.DateDeleted == null && stockIds.Contains(x.Id));
        }

        /// <summary>
        ///     Get a list of history for a given stock.
        /// </summary>
        /// <param name="stockId"></param>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UsePaging, UseSorting, UseFiltering, Authorize]
        public async Task<IQueryable<StockHistory>> GetStockHistoryAsync(
            long stockId,
            [Service] AppDbContext context,
            [Service] IResolverContext resolverContext
        )
        {
            var userId = resolverContext.GetUserId() ?? throw ErrorFactory.NotFound();
            var userType = resolverContext.GetUserType() ?? throw ErrorFactory.NotFound();
            resolverContext.SetUser(userId, userType);

            if (userType == Constants.UserType.User) {
                var auth = await resolverContext.AuthorizeAsync(Constants.Privilege.ManageStocks.Name);

                if (!auth.Succeeded) {
                    throw ErrorFactory.Unauthorized();
                }

                return context.StockHistory.Where(x => x.StockId == stockId);
            }

            // Fetch the stock IDs the user has access to
            var availableStocks = await context.Students
                    .Include(x => x.Group)
                    .ThenInclude(x => x.Instance)
                    .ThenInclude(x => x.StockInstances)
                    .Where(x => x.Id == userId)
                    .FirstOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            var hasAccess = availableStocks.Group.Instance.StockInstances.Any(x => x.StockId == stockId);

            if (!hasAccess) {
                throw ErrorFactory.NotFound();
            }

            return context.StockHistory.Where(x => x.StockId == stockId);
        }

        /// <summary>
        ///     Get all deleted stocks.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UsePaging, UseSelection, UseSorting, UseFiltering,
         Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public IQueryable<Stock> GetDeletedStocks([Service] AppDbContext context)
        {
            return context.Stocks.Where(x => x.DateDeleted != null);
        }
    }
}