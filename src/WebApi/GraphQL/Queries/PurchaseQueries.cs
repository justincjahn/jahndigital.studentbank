using System.Linq;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using JahnDigital.StudentBank.WebApi.Extensions;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Queries
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
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting, Authorize]
        public IQueryable<StudentPurchase> GetPurchases(
            [ScopedService] AppDbContext context,
            [Service] IResolverContext resolverContext
        )
        {
            if (resolverContext.GetUserType() == UserType.User) return context.StudentPurchases;

            return context
                .StudentPurchases
                .Where(x => x.StudentId == resolverContext.GetUserId());
        }
    }
}
