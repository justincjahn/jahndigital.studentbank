using System.Linq;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    /// <summary>
    ///     Allows students to list their purchases and admins to list all purchases.
    /// </summary>
    [ExtendObjectType("Query")]
    public class PurchaseQueries
    {
        /// <summary>
        ///     Get the purchases the user has available to them.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UsePaging, UseProjection, UseFiltering, UseSorting, Authorize]
        [UseDbContext(typeof(AppDbContext))]
        public IQueryable<StudentPurchase> GetPurchases(
            [ScopedService] AppDbContext context,
            [Service] IResolverContext resolverContext
        )
        {
            var userType = resolverContext.GetUserType() ?? throw ErrorFactory.Unauthorized();
            var userId = resolverContext.GetUserId() ?? throw ErrorFactory.Unauthorized();
            resolverContext.SetUser(userId, userType);

            if (userType == Constants.UserType.User) {
                return context.StudentPurchases;
            }

            return context.StudentPurchases.Where(x => x.StudentId == userId);
        }
    }
}
