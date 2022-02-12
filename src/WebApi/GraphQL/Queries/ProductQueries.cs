using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Queries
{
    /// <summary>
    ///     Allows students to view products assigned to their group and admins to list all products.
    /// </summary>
    [ExtendObjectType("Query")]
    public class ProductQueries
    {
        /// <summary>
        ///     Lists all products available to a given student.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         Authorize]
        public async Task<IQueryable<Product>> GetProductsAsync(
            [ScopedService] AppDbContext context,
            [Service] IResolverContext resolverContext
        )
        {
            UserType? userType = resolverContext.GetUserType() ?? throw ErrorFactory.Unauthorized();
            long userId = resolverContext.GetUserId() ?? throw ErrorFactory.Unauthorized();
            resolverContext.SetUser(userId, userType);

            if (userType == UserType.User)
            {
                return context.Products.Where(x => x.DateDeleted == null);
            }

            // Fetch the product IDs the user has access to
            Student? student = await context.Students
                    .Include(x => x.Group)
                    .ThenInclude(x => x.Instance)
                    .ThenInclude(x => x.ProductInstances)
                    .Where(x => x.Id == userId)
                    .FirstOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            IEnumerable<long>? productIds = student.Group.Instance.ProductInstances.Select(x => x.ProductId);

            return context.Products.Where(x => productIds.Contains(x.Id) && x.DateDeleted == null);
        }

        /// <summary>
        ///     Get a list of deleted products if authorized (Manage Products).
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_PRODUCTS)]
        public IQueryable<Product> GetDeletedProducts([ScopedService] AppDbContext context)
        {
            return context.Products.Where(x => x.DateDeleted != null);
        }
    }
}
