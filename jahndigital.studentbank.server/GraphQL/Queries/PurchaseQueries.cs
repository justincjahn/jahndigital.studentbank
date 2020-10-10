using System.Linq;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using jahndigital.studentbank.dal.Contexts;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    /// <summary>
    /// Allows students to list their purchases and admins to list all purchases.
    /// </summary>
    [ExtendObjectType(Name = "Query")]
    public class PurchaseQueries
    {
        /// <summary>
        /// Get the purchases the user has available to them.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UsePaging, UseFiltering, UseSelection, UseSorting, Authorize]
        public IQueryable<dal.Entities.StudentPurchase> GetPurchases(
            [Service]AppDbContext context,
            [Service]IResolverContext resolverContext
        ) {
            var userType = resolverContext.GetUserType() ?? throw ErrorFactory.Unauthorized();
            var userId = resolverContext.GetUserId() ?? throw ErrorFactory.Unauthorized();
            resolverContext.SetUser(userId, userType);
            if (userType == Constants.UserType.User) return context.StudentPurchases;
            return context.StudentPurchases.Where(x => x.StudentId == userId);
        }
    }
}
