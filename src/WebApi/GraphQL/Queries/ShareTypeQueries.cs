using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.ShareTypes.Queries.GetShareTypes;
using JahnDigital.StudentBank.Application.ShareTypes.Queries.GetShareTypesForStudent;
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
    public class ShareTypeQueries : RequestBase
    {
        /// <summary>
        ///     Get share type information available to the student or user.
        /// </summary>
        /// <param name="instances">A list of instances to filter the list of share types to.</param>
        /// <param name="resolverContext"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UsePaging, UseProjection, UseFiltering, UseSorting, Authorize]
        public async Task<IQueryable<ShareType>> GetShareTypesAsync(
            IEnumerable<long>? instances,
            [SchemaService] IResolverContext resolverContext,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            resolverContext.SetDataOwner();

            if (resolverContext.GetUserType() == UserType.User)
            {
                await resolverContext.AssertAuthorizedAsync(Privilege.ManageShareTypes.Name);
                return await mediatr.Send(new GetShareTypesQuery(instances), cancellationToken);
            }

            return await mediatr.Send(new GetShareTypesForStudentQuery(resolverContext.GetUserId()), cancellationToken);
        }

        /// <summary>
        ///     Get share type information.
        /// </summary>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UsePaging, UseProjection, UseFiltering, UseSorting, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public async Task<IQueryable<ShareType>> GetDeletedShareTypesAsync(
            [Service] ISender mediatr,
            CancellationToken cancellationToken
         )
        {
            return await mediatr.Send(new GetShareTypesQuery(null, true), cancellationToken);
        }
    }
}
