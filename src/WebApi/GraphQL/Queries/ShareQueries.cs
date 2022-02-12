using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.Infrastructure.Persistence;
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
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         HotChocolate.AspNetCore.Authorization.Authorize]
        public async Task<IQueryable<Share>> GetShares(
            [ScopedService] AppDbContext context,
            [Service] IResolverContext resolverContext
        )
        {
            UserType? userType = resolverContext.GetUserType() ?? throw ErrorFactory.Unauthorized();
            long userId = resolverContext.GetUserId() ?? throw ErrorFactory.Unauthorized();
            resolverContext.SetUser(userId, userType);

            if (userType == UserType.User)
            {
                AuthorizationResult? auth = await resolverContext.AuthorizeAsync(Privilege.ManageShares.Name);

                if (!auth.Succeeded)
                {
                    throw ErrorFactory.Unauthorized();
                }

                return context.Shares.AsQueryable();
            }

            return context.Shares.Where(x => x.StudentId == userId);
        }

        /// <summary>
        ///     Get deleted shares (if authorized).
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         HotChocolate.AspNetCore.Authorization.Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARES)]
        public IQueryable<Share> GetDeletedShares([ScopedService] AppDbContext context)
        {
            return context.Shares.Where(x => x.DateDeleted != null);
        }
    }
}