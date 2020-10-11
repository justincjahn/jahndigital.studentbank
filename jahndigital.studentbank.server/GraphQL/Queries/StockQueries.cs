using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using jahndigital.studentbank.dal.Contexts;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    [ExtendObjectType(Name = "Query")]
    public class StockQueries
    {
        /// <summary>
        /// Get a list of stocks available to the user.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UsePaging, UseSelection, UseSorting, UseFiltering, Authorize]
        public async Task<IQueryable<dal.Entities.Stock>> GetStocksAsync(
            [Service]AppDbContext context,
            [Service]IResolverContext resolverContext
        ) {
            var userType = resolverContext.GetUserType() ?? throw ErrorFactory.Unauthorized();
            var userId = resolverContext.GetUserId() ?? throw ErrorFactory.Unauthorized();
            resolverContext.SetUser(userId, userType);

            if (userType == Constants.UserType.User) {
                var auth = await resolverContext.AuthorizeAsync(Constants.Privilege.ManageStocks.Name);
                if (!auth.Succeeded) throw ErrorFactory.Unauthorized();
                return context.Stocks.Where(x => x.DateDeleted == null);
            }

            var student = await context.Students
                .Include(x => x.Group)
                .Where(x => x.Id == userId)
                .FirstOrDefaultAsync()
            ?? throw ErrorFactory.NotFound();

            return context.Stocks.Where(x => x.DateDeleted == null && x.InstanceId == student.Group.InstanceId);
        }

        /// <summary>
        /// Get all deleted stocks.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UsePaging, UseSelection, UseSorting, UseFiltering,
        Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public IQueryable<dal.Entities.Stock> GetDeletedStocks([Service]AppDbContext context) =>
            context.Stocks.Where(x => x.DateDeleted != null);
    }
}
