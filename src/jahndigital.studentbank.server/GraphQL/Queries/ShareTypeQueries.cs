using System.Collections.Generic;
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
using Microsoft.EntityFrameworkCore;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    [ExtendObjectType("Query")]
    public class ShareTypeQueries
    {
        /// <summary>
        ///     Get share type information available to the student or user.
        /// </summary>
        /// <param name="instances">A list of instances to filter the list of share types to.</param>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         HotChocolate.AspNetCore.Authorization.Authorize]
        public async Task<IQueryable<ShareType>> GetShareTypesAsync(
            IEnumerable<long>? instances,
            [ScopedService] AppDbContext context,
            [Service] IResolverContext resolverContext
        )
        {
            UserType? userType = resolverContext.GetUserType() ?? throw ErrorFactory.Unauthorized();
            long userId = resolverContext.GetUserId() ?? throw ErrorFactory.Unauthorized();
            resolverContext.SetUser(userId, userType);

            if (userType == UserType.User)
            {
                AuthorizationResult? auth =
                    await resolverContext.AuthorizeAsync(Privilege.ManageShareTypes.Name);

                if (!auth.Succeeded)
                {
                    throw ErrorFactory.Unauthorized();
                }

                IQueryable<ShareType>? shareTypes = context.ShareTypes.Where(x => x.DateDeleted == null);

                if (instances != null)
                {
                    shareTypes = shareTypes.Where(x => x.ShareTypeInstances.Any(x => instances.Contains(x.InstanceId)));
                }

                return shareTypes;
            }

            // Fetch the share type IDs the student has access to
            Student? shares = await context.Students
                    .Include(x => x.Group)
                    .ThenInclude(x => x.Instance)
                    .ThenInclude(x => x.ShareTypeInstances)
                    .Where(x => x.Id == userId)
                    .FirstOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            IEnumerable<long>? shareTypeIds = shares.Group.Instance.ShareTypeInstances.Select(x => x.ShareTypeId);

            return context.ShareTypes.Where(x => x.DateDeleted == null && shareTypeIds.Contains(x.Id));
        }

        /// <summary>
        ///     Get share type information.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         HotChocolate.AspNetCore.Authorization.Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public IQueryable<ShareType> GetDeletedShareTypes([ScopedService] AppDbContext context)
        {
            return context.ShareTypes.Where(x => x.DateDeleted != null);
        }
    }
}
