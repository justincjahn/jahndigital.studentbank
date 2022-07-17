using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.StudentPurchases.Queries.GetStudentPurchases;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
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
        /// <summary>
        ///     Get the purchases the user has available to them.
        /// </summary>
        /// <param name="resolverContext"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UsePaging, UseProjection, UseFiltering, UseSorting, Authorize]
        public async Task<IQueryable<StudentPurchase>> GetPurchasesAsync(
            [SchemaService] IResolverContext resolverContext,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            if (resolverContext.GetUserType() == UserType.User)
            {
                return await mediatr.Send(new GetStudentPurchasesQuery(), cancellationToken);
            }

            return await mediatr.Send(new GetStudentPurchasesQuery(resolverContext.GetUserId()), cancellationToken);
        }
    }
}
