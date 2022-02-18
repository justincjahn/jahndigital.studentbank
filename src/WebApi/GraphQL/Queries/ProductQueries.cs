using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Products.Queries.GetProducts;
using JahnDigital.StudentBank.Application.Products.Queries.GetStudentProducts;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.WebApi.Extensions;
using JahnDigital.StudentBank.WebApi.GraphQL.Common;
using MediatR;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Queries
{
    /// <summary>
    ///     Allows students to view products assigned to their group and admins to list all products.
    /// </summary>
    [ExtendObjectType("Query")]
    public class ProductQueries : RequestBase
    {
        /// <summary>
        ///     Lists all products available to a given student.
        /// </summary>
        /// <param name="resolverContext"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UsePaging, UseProjection, UseFiltering, UseSorting, Authorize]
        public async Task<IQueryable<Product>> GetProductsAsync(
            [Service] IResolverContext resolverContext,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            if (resolverContext.GetUserType() == UserType.User)
            {
                return await mediatr.Send(new GetProductsQuery(), cancellationToken);
            }

            return await mediatr.Send(new GetStudentProducts(resolverContext.GetUserId()), cancellationToken);
        }

        /// <summary>
        ///     Get a list of deleted products if authorized (Manage Products).
        /// </summary>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UsePaging, UseProjection, UseFiltering, UseSorting, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_PRODUCTS)]
        public async Task<IQueryable<Product>> GetDeletedProductsAsync(
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            return await mediatr.Send(new GetProductsQuery(true), cancellationToken);
        }
    }
}
