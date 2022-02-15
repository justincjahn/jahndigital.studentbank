using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Shares.Queries.GetShares;
using JahnDigital.StudentBank.Application.Shares.Queries.GetSharesForStudent;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.Infrastructure.Persistence;
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
        public ShareQueries(ISender mediatr) : base(mediatr) { }

        /// <summary>
        ///     Get shares for the currently active user.
        /// </summary>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting, Authorize]
        public async Task<IQueryable<Share>> GetSharesAsync([Service] IResolverContext resolverContext)
        {
            resolverContext.SetDataOwner();

            if (resolverContext.GetUserType() != UserType.User)
            {
                return await _mediatr.Send(new GetSharesForStudent(resolverContext.GetUserId()));
            }

            await resolverContext.AssertAuthorizedAsync(Privilege.ManageShares.Name);
            return await _mediatr.Send(new GetSharesQuery());
        }

        /// <summary>
        ///     Get deleted shares (if authorized).
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARES)]
        public async Task<IQueryable<Share>> GetDeletedSharesAsync([ScopedService] AppDbContext context)
        {
            return await _mediatr.Send(new GetSharesQuery(true));
        }
    }
}
