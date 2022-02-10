using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using jahndigital.studentbank.services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using static jahndigital.studentbank.utils.Constants;

namespace jahndigital.studentbank.server.Permissions
{
    /// <summary>
    ///     Validates that the user id contained in the request URL matches the authenticated user.
    /// </summary>
    internal class DataOwnerAuthorizationHandlerGraphQL : AuthorizationHandler<DataOwnerRequirement, IResolverContext>
    {
        public const string CTX_ISOWNER = "IsDataOwner";

        public const string CTX_USER_ID = "UserId";

        public const string CTX_USER_TYPE = "UserType";

        private readonly IHttpContextAccessor _httpContext;
        private readonly IRoleService _roleService;

        public DataOwnerAuthorizationHandlerGraphQL(IHttpContextAccessor context, IRoleService roleService)
        {
            _httpContext = context;
            _roleService = roleService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            DataOwnerRequirement requirement, IResolverContext resource)
        {
            // Make sure the user isn't preauthenticated
            if (PreauthorizationHandler.AssertPreauthenticated(context))
            {
                return;
            }

            if (IsDataOwner(context, resource))
            {
                context.Succeed(requirement);

                return;
            }

            bool hasPermission = await HasPermission(context, requirement, resource);

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
        }

        /// <summary>
        ///     Determine if the current user is the owner of the resource.
        /// </summary>
        /// <remarks>
        ///     By convention the a request param called 'userId' should be used for user resources
        ///     and 'studentId' should be used for student resources.
        /// </remarks>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns>true if the current user/student is a data owner.</returns>
        private bool IsDataOwner(AuthorizationHandlerContext context, IResolverContext resolverContext)
        {
            // If we've cached the result, return it
            if (resolverContext.ScopedContextData.ContainsKey(CTX_ISOWNER))
            {
                return (bool)resolverContext.ScopedContextData[CTX_ISOWNER]!;
            }

            // var route = resolverContext.FieldSelection.Arguments;
            IReadOnlyList<ArgumentNode>? route = resolverContext.Selection.SyntaxNode.Arguments;

            string userId = string.Empty;
            UserType? userType = null;

            resolverContext.ScopedContextData = resolverContext.ScopedContextData.SetItem(CTX_ISOWNER, false);

            // Check to see if the caller has specified the user ID and type
            if (resolverContext.ScopedContextData.ContainsKey(CTX_USER_ID)
                && resolverContext.ScopedContextData.ContainsKey(CTX_USER_TYPE))
            {
                userId = resolverContext.ScopedContextData[CTX_USER_ID]?.ToString() ?? "";
                userType = (UserType)resolverContext.ScopedContextData[CTX_USER_TYPE]!;
            }
            else
            {
                ArgumentNode? arg = route.FirstOrDefault(x => x.Name.Value == "userId");

                if (arg != null)
                {
                    userType = UserType.User;
                    userId = arg.Value.Value!.ToString()!;
                }

                arg = route.FirstOrDefault(x => x.Name.Value == "studentId");

                if (arg != null)
                {
                    userType = UserType.Student;
                    userId = arg.Value.Value!.ToString()!;
                }
            }

            if (userType == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            Claim? userIdClaim = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return false;
            }

            Claim? userTypeClaim = context.User.Claims.FirstOrDefault(x => x.Type == Auth.CLAIM_USER_TYPE);

            if (userTypeClaim == null)
            {
                return false;
            }

            int routeId, claimId;

            if (!int.TryParse(userId, out routeId))
            {
                return false;
            }

            if (!int.TryParse(userIdClaim.Value, out claimId))
            {
                return false;
            }

            // Assert the claim type matches the user type from the URL convention
            if (userType.Name != userTypeClaim.Value)
            {
                return false;
            }

            if (routeId == claimId)
            {
                resolverContext.ScopedContextData = resolverContext.ScopedContextData.SetItem(CTX_ISOWNER, true);

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Determine if the current user has permissions to access the resource.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        private async Task<bool> HasPermission(AuthorizationHandlerContext context, DataOwnerRequirement requirement,
            IResolverContext resolverContext)
        {
            if (requirement.Permissions.Count() < 1)
            {
                return false;
            }

            Claim? role = context.User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Role);

            if (role == null)
            {
                return false;
            }

            return await _roleService.HasPermissionAsync(role.Value, requirement.Permissions);
        }
    }
}