using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using JahnDigital.StudentBank.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Queries
{
    [ExtendObjectType("Query")]
    public class ShareQueries
    {
        /// <summary>
        ///     Get shares for the currently active user.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting, Authorize]
        public async Task<IQueryable<Share>> GetShares(
            [ScopedService] AppDbContext context,
            [Service] IResolverContext resolverContext
        )
        {
            resolverContext.SetDataOwner();

            if (resolverContext.GetUserType() == UserType.User)
            {
                await resolverContext.AssertAuthorizedAsync(Privilege.ManageShares.Name);
                return context.Shares.AsQueryable();
            }

            return context
                .Shares
                .Where(x => x.StudentId == resolverContext.GetUserId());
        }

        /// <summary>
        ///     Get deleted shares (if authorized).
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARES)]
        public IQueryable<Share> GetDeletedShares([ScopedService] AppDbContext context)
        {
            return context
                .Shares
                .Where(x => x.DateDeleted != null);
        }
    }
}
