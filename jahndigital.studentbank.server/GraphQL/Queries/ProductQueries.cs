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
    /// Allows students to view products assigned to their group and admins to list all products.
    /// </summary>
    [ExtendObjectType(Name = "Query")]
    public class ProductQueries
    {
        /// <summary>
        /// Lists all products available to a given student.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UsePaging, UseSelection, UseSorting, UseFiltering, Authorize]
        public async Task<IQueryable<dal.Entities.Product>> GetProductsAsync(
            [Service]AppDbContext context,
            [Service]IResolverContext resolverContext
        ) {
            var userType = resolverContext.GetUserType() ?? throw ErrorFactory.Unauthorized();
            var userId = resolverContext.GetUserId() ?? throw ErrorFactory.Unauthorized();
            resolverContext.SetUser(userId, userType);

            if (userType == Constants.UserType.User) {
                return context.Products.Where(x => x.DateDeleted == null);
            }

            // Fetch the product IDs the user has access to
            var student = await context.Students
                .Include(x => x.Group)
                    .ThenInclude(x => x.Instance)
                        .ThenInclude(x => x.ProductInstances)
                .Where(x => x.Id == userId)
                .FirstOrDefaultAsync()
            ?? throw ErrorFactory.NotFound();

            var productIds = student.Group.Instance.ProductInstances.Select(x => x.ProductId);
            return context.Products.Where(x => productIds.Contains(x.Id) && x.DateDeleted == null);
        }
        
        /// <summary>
        /// Get a list of deleted products if authorized (Manage Products).
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UsePaging, UseSelection, UseSorting, UseFiltering,
        Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_PRODUCTS)]
        public IQueryable<dal.Entities.Product> GetDeletedProducts([Service]AppDbContext context) =>
            context.Products.Where(x => x.DateDeleted != null);
    }
}
