using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Execution;
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
        /// Get a list of available stocks.
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UsePaging, UseFiltering, UseSelection, UseSorting,
        Authorize(Policy = Constants.AuthPolicy.DataOwner + "<" + Constants.Privilege.PRIVILEGE_MANAGE_STOCKS + ">")]
        public async Task<IQueryable<dal.Entities.Stock>> GetAvailableStocks(long studentId, [Service]AppDbContext context)
        {
            long? instanceId = await context.Students
                .Include(x => x.Group)
                .Where(x => x.Id == studentId)
                .Select(x => x.Group.InstanceId)
                .FirstOrDefaultAsync();
            
            if (instanceId == null) {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("Student not found.")
                        .SetCode("NOT_FOUND")
                        .Build()
                );
            }

            return context.Stocks.Where(x => x.InstanceId == instanceId);
        }

        /// <summary>
        /// Get all stocks available for an instance if authorized (Manage Stocks).
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UsePaging, UseFiltering, UseSelection, UseSorting,
        Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public IQueryable<dal.Entities.Stock> GetStocks(long instanceId, [Service]AppDbContext context) =>
            context.Stocks.Where(x => x.InstanceId == instanceId);
    }
}
