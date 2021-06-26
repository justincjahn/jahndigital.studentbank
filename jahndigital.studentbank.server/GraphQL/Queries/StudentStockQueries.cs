using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.utils;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    [ExtendObjectType(Name = "Query")]
    public class StudentStockQueries
    {
        /// <summary>
        /// Get the stocks purchased by the student specified.
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UsePaging, UseSelection, UseSorting, UseFiltering, Authorize]
        public async Task<IQueryable<dal.Entities.StudentStock>> GetStudentStocksAsync(
            long studentId,
            [Service] AppDbContext context,
            [Service] IResolverContext resolverContext
        ) {
            var id = resolverContext.GetUserId() ?? throw ErrorFactory.NotFound();
            var type = resolverContext.GetUserType() ?? throw ErrorFactory.NotFound();
            resolverContext.SetUser(studentId, type);

            var auth = await resolverContext
                .AuthorizeAsync($"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManageStocks}>");

            if (!auth.Succeeded) throw ErrorFactory.Unauthorized();

            return context.StudentStocks.Where(x => x.StudentId == studentId);
        }

        /// <summary>
        /// Get the purchase history for a student's stock.
        /// </summary>
        /// <param name="studentStockId"></param>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UsePaging, UseSelection, UseSorting, UseFiltering, Authorize]
        public async Task<IQueryable<dal.Entities.StudentStockHistory>> GetStudentStockHistoryAsync(
            long studentStockId,
            [Service] AppDbContext context,
            [Service] IResolverContext resolverContext
        ) {
            var type = resolverContext.GetUserType() ?? throw ErrorFactory.NotFound();

            var studentStock = await context.StudentStocks
                .Where(x => x.Id == studentStockId)
                .FirstOrDefaultAsync()
            ?? throw ErrorFactory.NotFound();

            resolverContext.SetUser(studentStock.StudentId, type);

            var auth = await resolverContext
                .AuthorizeAsync($"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManageStocks}>");

            if (!auth.Succeeded) throw ErrorFactory.Unauthorized();

            return context.StudentStockHistory.Where(x => x.StudentStockId == studentStockId);
        }
    }
}
