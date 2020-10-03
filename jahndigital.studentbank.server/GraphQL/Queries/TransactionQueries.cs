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
    /// <summary>
    /// 
    /// </summary>
    [ExtendObjectType(Name = "Query")]
    public class TransactionQueries
    {
        /// <summary>
        /// Get transactions by Student ID and Share ID
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="shareId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UsePaging, UseFiltering, UseSorting,
        Authorize(Policy = Constants.AuthPolicy.DataOwner + "<" + Constants.Privilege.PRIVILEGE_MANAGE_TRANSACTIONS + ">")]
        public async Task<IQueryable<dal.Entities.Transaction>> GetTransactionsAsync(long studentId, long shareId, [Service]AppDbContext context) {
            // The auth policy is there, but this step ensures students can't "fake" their studentId
            var share = await context.Shares.Where(x => x.StudentId == studentId && x.Id == shareId).AnyAsync();
            if (!share) {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("You are not authorized to access this resource.")
                        .SetCode("UNAUTHORIZED")
                        .Build()
                );
            }

            return context.Transactions.Where(x => x.TargetShareId == shareId);
        }
    }
}
