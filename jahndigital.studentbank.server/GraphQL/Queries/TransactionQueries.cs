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
    /// <summary>
    /// 
    /// </summary>
    [ExtendObjectType(Name = "Query")]
    public class TransactionQueries
    {
        /// <summary>
        /// Get transactions by Student ID and Share ID
        /// </summary>
        /// <param name="shareId"></param>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UsePaging, UseSorting, UseFiltering,Authorize]
        public async Task<IQueryable<dal.Entities.Transaction>> GetTransactionsAsync(
            long shareId,
            [Service] AppDbContext context,
            [Service] IResolverContext resolverContext
        ) {
            var id = resolverContext.GetUserId() ?? throw ErrorFactory.NotFound();
            var type = resolverContext.GetUserType() ?? throw ErrorFactory.NotFound();

            var share = await context.Shares
                .Where(x => x.Id == shareId)
                .FirstOrDefaultAsync()
            ?? throw ErrorFactory.NotFound();

            resolverContext.SetUser(share.StudentId, Constants.UserType.Student);

            var auth = await resolverContext
                .AuthorizeAsync($"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManageTransactions}>");

            if (!auth.Succeeded) throw ErrorFactory.Unauthorized();

            return context.Transactions.Where(x => x.TargetShareId == shareId);
        }
    }
}
