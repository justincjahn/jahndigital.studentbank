using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Products.Commands.LinkUnlinkProduct;
using JahnDigital.StudentBank.Application.Products.Commands.NewProduct;
using JahnDigital.StudentBank.Application.Products.Commands.UpdateProduct;
using JahnDigital.StudentBank.Application.Products.Queries.GetProduct;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.WebApi.Models;
using MediatR;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Mutations
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
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_PURCHASES)]
        public async Task<IQueryable<Product>> NewProductAsync(
            NewProductRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            var productId = await mediatr.Send(new NewProductCommand(
                input.Name,
                input.Description,
                input.Cost,
                input.IsLimitedQuantity,
                input.Quantity
            ), cancellationToken);

            return await mediatr.Send(new GetProductQuery(productId), cancellationToken);
        }

        /// <summary>
        ///     Update a <see cref="Product" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_PRODUCTS)]
        public async Task<IQueryable<Product>> UpdateProductAsync(
            UpdateProductRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new UpdateProductCommand
            {
                Id = input.Id,
                Name = input.Name,
                Cost = input.Cost,
                Description = input.Description,
                IsLimitedQuantity = input.IsLimitedQuantity,
                Quantity = input.Quantity
            }, cancellationToken);

            return await mediatr.Send(new GetProductQuery(input.Id), cancellationToken);
        }

        /// <summary>
        ///     Link a <see cref="Product" /> with the provided <see cref="Group" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_PRODUCTS)]
        public async Task<IQueryable<Product>> LinkProductAsync(
            LinkProductRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new LinkUnlinkProductCommand(input.ProductId, input.InstanceId, true), cancellationToken);
            return await mediatr.Send(new GetProductQuery(input.ProductId), cancellationToken);
        }

        /// <summary>
        ///     Unlink a <see cref="Product" /> from the provided <see cref="Group" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_PRODUCTS)]
        public async Task<IQueryable<Product>> UnlinkProductAsync(
            LinkProductRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new LinkUnlinkProductCommand(input.ProductId, input.InstanceId, false), cancellationToken);
            return await mediatr.Send(new GetProductQuery(input.ProductId), cancellationToken);
        }

        /// <summary>
        ///     Soft-delete a <see cref="Product" />.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Policy = Privilege.PRIVILEGE_MANAGE_PRODUCTS)]
        public async Task<bool> DeleteProductAsync(
            long id,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new UpdateProductCommand { Id = id, Deleted = true }, cancellationToken);
            return true;
        }

        /// <summary>
        ///     Restore a soft-deleted <see cref="Product" />.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_PRODUCTS)]
        public async Task<IQueryable<Product>> RestoreProductAsync(
            long id,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new UpdateProductCommand() { Id = id }, cancellationToken);
            return await mediatr.Send(new GetProductQuery(id), cancellationToken);
        }
    }
}
