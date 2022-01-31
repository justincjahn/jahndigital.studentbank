using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    /// <summary>
    /// </summary>
    [ExtendObjectType("Query")]
    public class TransactionQueries
    {
        /// <summary>
        ///     Get transactions by Student ID and Share ID
        /// </summary>
        /// <param name="shareId"></param>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseFiltering, UseSorting,
         HotChocolate.AspNetCore.Authorization.Authorize]
        public async Task<IQueryable<Transaction>> GetTransactionsAsync(
            long shareId,
            [ScopedService] AppDbContext context,
            [Service] IResolverContext resolverContext
        )
        {
            long id = resolverContext.GetUserId() ?? throw ErrorFactory.NotFound();
            Constants.UserType? type = resolverContext.GetUserType() ?? throw ErrorFactory.NotFound();

            Share? share = await context.Shares
                    .Where(x => x.Id == shareId)
                    .FirstOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            resolverContext.SetUser(share.StudentId, Constants.UserType.Student);

            AuthorizationResult? auth = await resolverContext
                .AuthorizeAsync($"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManageTransactions}>");

            if (!auth.Succeeded)
            {
                throw ErrorFactory.Unauthorized();
            }

            return context.Transactions.Where(x => x.TargetShareId == shareId);
        }
    }
}
