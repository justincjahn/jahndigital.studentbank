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
    /// Allows students to view products assigned to their group and admins to list all products.
    /// </summary>
    [ExtendObjectType(Name = "Query")]
    public class ProductQueries
    {
        /// <summary>
        /// Lists all products available to a given student.
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UsePaging, UseFiltering, UseSorting, UseSelection,
        Authorize(Policy = Constants.AuthPolicy.DataOwner + "<" + Constants.Privilege.PRIVILEGE_MANAGE_PRODUCTS + ">")]
        public async Task<IQueryable<dal.Entities.Product>> GetAvailableProductsAsync(long studentId, [Service]AppDbContext context)
        {
            var student = await context.Students
                .Include(x => x.Group)
                .Where(x => x.Id == studentId)
                .SingleOrDefaultAsync();

            if (student == null) {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("Student not found.")
                        .SetCode("NOT_FOUND")
                        .Build()
                );
            }

            var productGroups = await context.ProductGroups
                .Where(x => x.GroupId == student.Group.Id)
                .Select(x => x.ProductId)
                .ToListAsync();

            return context.Products.Where(x => productGroups.Contains(x.Id));
        }

        /// <summary>
        /// Get a list of all products if authorized (Manage Products).
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UsePaging, UseFiltering, UseSorting, UseSelection,
        Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_PRODUCTS)]
        public IQueryable<dal.Entities.Product> GetProducts([Service]AppDbContext context) =>
            context.Products;
    }
}
