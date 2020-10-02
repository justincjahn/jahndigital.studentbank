using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    /// <summary>
    /// 
    /// </summary>
    [ExtendObjectType(Name = "Query")]
    public class TransactionQueries
    {
        /// <summary>
        /// Get transactions by Share ID
        /// </summary>
        /// <param name="shareId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public IQueryable<dal.Entities.Transaction> GetTransactions(long shareId, [Service]AppDbContext context) =>
            context.Transactions.Where(x => x.TargetShareId == shareId);
    }
}
