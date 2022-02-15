using System.Linq;
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
using JahnDigital.StudentBank.Infrastructure.Persistence;
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
        public ProductQueries(ISender mediatr) : base(mediatr) { }

        /// <summary>
        ///     Lists all products available to a given student.
        /// </summary>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting, Authorize]
        public async Task<IQueryable<Product>> GetProductsAsync([Service] IResolverContext resolverContext)
        {
            if (resolverContext.GetUserType() == UserType.User)
            {
                return await _mediatr.Send(new GetProductsQuery());
            }

            return await _mediatr.Send(new GetStudentProducts(resolverContext.GetUserId()));
        }

        /// <summary>
        ///     Get a list of deleted products if authorized (Manage Products).
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_PRODUCTS)]
        public async Task<IQueryable<Product>> GetDeletedProductsAsync([ScopedService] AppDbContext context)
        {
            return await _mediatr.Send(new GetProductsQuery(true));
        }
    }
}
