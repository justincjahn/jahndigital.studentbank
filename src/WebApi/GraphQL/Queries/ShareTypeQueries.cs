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
using JahnDigital.StudentBank.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Queries
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
            resolverContext.SetDataOwner();

            if (resolverContext.GetUserType() == UserType.User)
            {
                await resolverContext.AssertAuthorizedAsync(Privilege.ManageShareTypes.Name);

                var shareTypes = context
                    .ShareTypes
                    .Where(x => x.DateDeleted == null);

                if (instances != null)
                {
                    shareTypes = shareTypes
                        .Where(x => x.ShareTypeInstances.Any(y => instances.Contains(y.InstanceId)));
                }

                return shareTypes;
            }

            // Fetch the share type IDs the student has access to
            Student? shares = await context
                    .Students
                    .Include(x => x.Group)
                    .ThenInclude(x => x.Instance)
                    .ThenInclude(x => x.ShareTypeInstances)
                    .Where(x => x.Id == resolverContext.GetUserId())
                    .FirstOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            var shareTypeIds = shares
                .Group
                .Instance
                .ShareTypeInstances
                .Select(x => x.ShareTypeId);

            return context
                .ShareTypes
                .Where(x => x.DateDeleted == null && shareTypeIds.Contains(x.Id));
        }

        /// <summary>
        ///     Get share type information.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_SHARE_TYPES)]
        public IQueryable<ShareType> GetDeletedShareTypes([ScopedService] AppDbContext context)
        {
            return context
                .ShareTypes
                .Where(x => x.DateDeleted != null);
        }
    }
}
