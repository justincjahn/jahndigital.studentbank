using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Execution;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.server.Models;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.server.GraphQL.Mutations
{
    /// <summary>
    /// CRUD operations for <see cref="dal.Entities.Product"/> and <see cref="dal.Entities.ProductGroup"/>
    /// </summary>
    [ExtendObjectType(Name = "Mutation")]
    public class ProductMutations
    {
        /// <summary>
        /// Create a new product.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_PURCHASES)]
        public async Task<IQueryable<dal.Entities.Product>> NewProductAsync(
            NewProductRequest input,
            [Service] AppDbContext context,
            [Service] IRequestContext requestContext
        ) {
            var hasProduct = await context.Products.AnyAsync(x => x.Name == input.Name);

            if (hasProduct) {
                throw ErrorFactory.QueryFailed("A product with the specified name already exists!");
            }

            var product = new dal.Entities.Product {
                Name = input.Name,
                Description = input.Description,
                Cost = input.Cost,
                IsLimitedQuantity = input.IsLimitedQuantity,
                Quantity = input.IsLimitedQuantity ? -1 : input.Quantity
            };

            try {
                context.Add(product);
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Products.Where(x => x.Id == product.Id);
        }

        /// <summary>
        /// Update a product.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_PRODUCTS)]
        public async Task<IQueryable<dal.Entities.Product>> UpdateProductAsync(
            UpdateProductRequest input,
            [Service] AppDbContext context
        ) {
            var product = await context.Products
                .Where(x => x.Id == input.Id)
                .FirstOrDefaultAsync()
            ?? throw ErrorFactory.NotFound();

            product.Cost = input.Cost ?? product.Cost;
            product.IsLimitedQuantity = input.IsLimitedQuantity ?? product.IsLimitedQuantity;
            product.Quantity = product.IsLimitedQuantity ? -1 : input.Quantity ?? product.Quantity;
            product.Description = input.Description ?? product.Description;

            if (input.Name != null) {
                var hasName = await context.Products
                    .AnyAsync(x => x.Id != product.Id && x.Name == input.Name);
                
                if (hasName) throw ErrorFactory.QueryFailed($"Product with name '{input.Name}' already exists!");

                product.Name = input.Name;
            }

            try {
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Products.Where(x => x.Id == product.Id);
        }

        /// <summary>
        /// Link a product with the provided group.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_PRODUCTS)]
        public async Task<bool> LinkProductAsync(
            LinkProductRequest input,
            [Service] AppDbContext context
        ) {
            var hasLink = await context.ProductGroups
                .AnyAsync(x => x.ProductId == input.ProductId && x.GroupId == input.GroupId);

            if (hasLink) throw ErrorFactory.QueryFailed("A link already exists!");

            var productGroup = new dal.Entities.ProductGroup {
                ProductId = input.ProductId,
                GroupId = input.GroupId
            };

            try {
                context.Add(productGroup);
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return true;
        }

        /// <summary>
        /// Unlink a product from the provided group.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_PRODUCTS)]
        public async Task<bool> UnlinkProductAsync(
            LinkProductRequest input,
            [Service] AppDbContext context
        ) {
            var link = await context.ProductGroups
                .Where(x => x.ProductId == input.ProductId && x.GroupId == input.GroupId)
                .FirstOrDefaultAsync()
            ?? throw ErrorFactory.NotFound();

            try {
                context.Remove(link);
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return true;
        }
    }
}
