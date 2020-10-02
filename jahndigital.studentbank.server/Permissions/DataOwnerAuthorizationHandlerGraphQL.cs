using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using jahndigital.studentbank.server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using static jahndigital.studentbank.server.Constants;

namespace jahndigital.studentbank.server.Permissions
{
    /// <summary>
    /// Validates that the user id contained in the request URL matches the authenticated user.
    /// </summary>
    internal class DataOwnerAuthorizationHandlerGraphQL : AuthorizationHandler<DataOwnerRequirement, IResolverContext>
    {
        const string CTXNAME = "IsDataOwner";

        private readonly IHttpContextAccessor _httpContext;
        private readonly IRoleService _roleService;

        public DataOwnerAuthorizationHandlerGraphQL(IHttpContextAccessor context, IRoleService roleService) {
            _httpContext = context;
            _roleService = roleService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DataOwnerRequirement requirement, IResolverContext resource)
        {
            if (IsDataOwner(context, resource)) {
                context.Succeed(requirement);
                return;
            }

            var hasPermission = await HasPermission(context, requirement, resource);
            if (hasPermission) context.Succeed(requirement);
        }

        /// <summary>
        /// Determine if the current user is the owner of the resource.
        /// </summary>
        /// <remarks>
        /// By convention the a request param called 'userId' should be used for user resources
        /// and 'studentId' should be used for student resources.
        /// </remarks>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns>true if the current user/student is a data owner.</returns>
        private bool IsDataOwner(AuthorizationHandlerContext context, IResolverContext resolverContext)
        {
            if (resolverContext.ScopedContextData.ContainsKey(CTXNAME)) {
                return (bool)resolverContext.ScopedContextData[CTXNAME];
            }

            var route = resolverContext.FieldSelection.Arguments;
            
            string userId = string.Empty;
            UserType? userType = null;

            resolverContext.ScopedContextData = resolverContext.ScopedContextData.SetItem(CTXNAME, false);

            HotChocolate.Language.ArgumentNode? arg = route.FirstOrDefault(x => x.Name.Value == "userId");
            if (arg != null) {
                userType = UserType.User;
                userId = arg.Value.Value.ToString()!;
            }

            arg = route.FirstOrDefault(x => x.Name.Value == "studentId");
            if (arg != null) {
                userType = UserType.Student;
                userId = arg.Value.Value.ToString()!;
            }

            if (userType == null) return false;
            if (string.IsNullOrWhiteSpace(userId)) return false;

            var userIdClaim = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return false;

            var userTypeClaim = context.User.Claims.FirstOrDefault(x => x.Type == Constants.Auth.CLAIM_USER_TYPE);
            if (userTypeClaim == null) return false;

            int routeId, claimId;
            if (!int.TryParse(userId, out routeId)) return false;
            if (!int.TryParse(userIdClaim.Value, out claimId)) return false;

            // Assert the claim type matches the user type from the URL convention
            if (userType.Name != userTypeClaim.Value) return false;

            if (routeId == claimId) {
                resolverContext.ScopedContextData = resolverContext.ScopedContextData.SetItem(CTXNAME, true);
                return true;
            }

            return false;
        }
    
        /// <summary>
        /// Determine if the current user has permissions to access the resource.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        private async Task<bool> HasPermission(AuthorizationHandlerContext context, DataOwnerRequirement requirement, IResolverContext resolverContext)
        {
            if (requirement.Permissions.Count() < 1) return false;
            var role = context.User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Role);
            if (role == null) return false;
            return await _roleService.HasPermissionAsync(role.Value, requirement.Permissions);
        }
    }
}
