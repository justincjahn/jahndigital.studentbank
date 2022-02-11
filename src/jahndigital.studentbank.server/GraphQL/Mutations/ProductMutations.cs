using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Execution;
using HotChocolate.Types;
using JahnDigital.StudentBank.Domain.Entities;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using jahndigital.studentbank.server.Models;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.server.GraphQL.Mutations
{
    /// <summary>
    ///     CRUD operations for <see cref="Product" /> and <see cref="ProductInstance" />
    /// </summary>
    [ExtendObjectType("Mutation")]
    public class ProductMutations
    {
        /// <summary>
        ///     Create a new <see cref="Product" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)),
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_PURCHASES)]
        public async Task<IQueryable<Product>> NewProductAsync(
            NewProductRequest input,
            [ScopedService] AppDbContext context,
            [Service] IRequestContext requestContext
        )
        {
            bool hasProduct = await context.Products.AnyAsync(x => x.Name == input.Name);

            if (hasProduct)
            {
                throw ErrorFactory.QueryFailed("A product with the specified name already exists!");
            }

            Product? product = new Product
            {
                Name = input.Name,
                Description = input.Description,
                Cost = input.Cost,
                IsLimitedQuantity = input.IsLimitedQuantity,
                Quantity = input.IsLimitedQuantity ? -1 : input.Quantity
            };

            try
            {
                context.Add(product);
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Products.Where(x => x.Id == product.Id);
        }

        /// <summary>
        ///     Update a <see cref="Product" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)),
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_PRODUCTS)]
        public async Task<IQueryable<Product>> UpdateProductAsync(
            UpdateProductRequest input,
            [ScopedService] AppDbContext context
        )
        {
            Product? product = await context.Products
                    .Where(x => x.Id == input.Id)
                    .FirstOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            product.Cost = input.Cost ?? product.Cost;
            product.IsLimitedQuantity = input.IsLimitedQuantity ?? product.IsLimitedQuantity;
            product.Quantity = product.IsLimitedQuantity ? -1 : input.Quantity ?? product.Quantity;
            product.Description = input.Description ?? product.Description;

            if (input.Name != null)
            {
                bool hasName = await context.Products
                    .AnyAsync(x => x.Id != product.Id && x.Name == input.Name);

                if (hasName)
                {
                    throw ErrorFactory.QueryFailed($"Product with name '{input.Name}' already exists!");
                }

                product.Name = input.Name;
            }

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                throw ErrorFactory.QueryFailed(e.InnerException?.Message ?? e.Message);
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Products.Where(x => x.Id == product.Id);
        }

        /// <summary>
        ///     Link a <see cref="Product" /> with the provided <see cref="dal.Entities.Group" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection,
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_PRODUCTS)]
        public async Task<IQueryable<Product>> LinkProductAsync(
            LinkProductRequest input,
            [ScopedService] AppDbContext context
        )
        {
            bool hasLink = await context.ProductInstances
                .AnyAsync(x => x.ProductId == input.ProductId && x.InstanceId == input.InstanceId);

            if (hasLink)
            {
                throw ErrorFactory.QueryFailed("A link already exists!");
            }

            ProductInstance? productInstance =
                new ProductInstance { ProductId = input.ProductId, InstanceId = input.InstanceId };

            try
            {
                context.Add(productInstance);
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Products.Where(x => x.Id == input.ProductId);
        }

        /// <summary>
        ///     Unlink a <see cref="Product" /> from the provided <see cref="dal.Entities.Group" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection,
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_PRODUCTS)]
        public async Task<IQueryable<Product>> UnlinkProductAsync(
            LinkProductRequest input,
            [ScopedService] AppDbContext context
        )
        {
            ProductInstance? link = await context.ProductInstances
                    .Where(x => x.ProductId == input.ProductId && x.InstanceId == input.InstanceId)
                    .FirstOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            try
            {
                context.Remove(link);
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Products.Where(x => x.Id == input.ProductId);
        }

        /// <summary>
        ///     Soft-delete a <see cref="Product" />.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)),
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_PRODUCTS)]
        public async Task<bool> DeleteProductAsync(
            long id,
            [ScopedService] AppDbContext context
        )
        {
            Product? product = await context.Products.FindAsync(id)
                ?? throw ErrorFactory.NotFound();

            product.DateDeleted = DateTime.UtcNow;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return true;
        }

        /// <summary>
        ///     Restore a soft-deleted <see cref="Product" />.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection,
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_PRODUCTS)]
        public async Task<IQueryable<Product>> RestoreProductAsync(
            long id,
            [ScopedService] AppDbContext context
        )
        {
            Product? product = await context.Products
                    .Where(x => x.Id == id && x.DateDeleted != null)
                    .SingleOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            product.DateDeleted = null;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Products.Where(x => x.Id == id);
        }
    }
}
