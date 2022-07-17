using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Shares.Queries.GetShares;
using JahnDigital.StudentBank.Application.Shares.Queries.GetSharesForStudent;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.WebApi.Extensions;
using JahnDigital.StudentBank.WebApi.GraphQL.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Queries
{
    [ExtendObjectType("Query")]
    public class ShareQueries : RequestBase
    {
        /// <summary>
        ///     Get shares for the currently active user.
        /// </summary>
        /// <param name="resolverContext"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UsePaging, UseProjection, UseFiltering, UseSorting, Authorize]
        public async Task<IQueryable<Share>> GetSharesAsync(
            [SchemaService] IResolverContext resolverContext,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            resolverContext.SetDataOwner();

            if (resolverContext.GetUserType() != UserType.User)
            {
                return await mediatr.Send(new GetSharesForStudent(resolverContext.GetUserId()), cancellationToken);
            }

            await resolverContext.AssertAuthorizedAsync(Privilege.ManageShares.Name);
            return await mediatr.Send(new GetSharesQuery(), cancellationToken);
        }

        /// <summary>
        ///     Get deleted shares (if authorized).
        /// </summary>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UsePaging, UseProjection, UseFiltering, UseSorting, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARES)]
        public async Task<IQueryable<Share>> GetDeletedSharesAsync(
            [Service] ISender mediatr,
            CancellationToken cancellationToken
         )
        {
            return await mediatr.Send(new GetSharesQuery(true), cancellationToken);
        }
    }
}
