using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.StudentPurchases.Queries.GetStudentPurchases;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using JahnDigital.StudentBank.WebApi.Extensions;
using JahnDigital.StudentBank.WebApi.GraphQL.Common;
using MediatR;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Queries
{
    /// <summary>
    ///     Allows students to list their purchases and admins to list all purchases.
    /// </summary>
    [ExtendObjectType("Query")]
    public class PurchaseQueries : RequestBase
    {
        public PurchaseQueries(ISender mediatr) : base(mediatr) { }

        /// <summary>
        ///     Get the purchases the user has available to them.
        /// </summary>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting, Authorize]
        public async Task<IQueryable<StudentPurchase>> GetPurchases([Service] IResolverContext resolverContext)
        {
            if (resolverContext.GetUserType() == UserType.User)
            {
                return await _mediatr.Send(new GetStudentPurchasesQuery());
            }

            return await _mediatr.Send(new GetStudentPurchasesQuery(resolverContext.GetUserId()));
        }
    }
}
