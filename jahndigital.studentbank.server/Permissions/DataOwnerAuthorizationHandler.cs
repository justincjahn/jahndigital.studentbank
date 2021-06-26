using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using jahndigital.studentbank.services.Interfaces;
using jahndigital.studentbank.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using static jahndigital.studentbank.utils.Constants;

namespace jahndigital.studentbank.server.Permissions
{
    /// <summary>
    /// Validates that the user id contained in the request URL matches the authenticated user.
    /// </summary>
    internal class DataOwnerAuthorizationHandler : AuthorizationHandler<DataOwnerRequirement>
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly IRoleService _roleService;

        public DataOwnerAuthorizationHandler(IHttpContextAccessor context, IRoleService roleService) {
            _httpContext = context;
            _roleService = roleService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DataOwnerRequirement requirement)
        {
            // Make sure the user isn't preauthenticated
            if (PreauthorizationHandler.AssertPreauthenticated(context)) return;

            if (IsDataOwner(context)) {
                context.Succeed(requirement);
                return;
            }

            var hasPermission = await HasPermission(context, requirement);
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
        /// <returns>true if the current user/student is a data owner.</returns>
        private bool IsDataOwner(AuthorizationHandlerContext context)
        {
            var route = _httpContext.HttpContext?.Request.RouteValues
                ?? throw new Exception("Unable to get HttpContext when calling IsDataOwner.");
            
            string userId = string.Empty;
            UserType? userType = null;

            if (route.ContainsKey("userId")) {
                userType = UserType.User;
                userId = route["userId"]?.ToString()!;
            }

            if (route.ContainsKey("studentId")) {
                userType = UserType.Student;
                userId = route["studentId"]?.ToString()!;
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

            if (routeId == claimId) return true;
            return false;
        }
    
        /// <summary>
        /// Determine if the current user has permissions to access the resource.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        private async Task<bool> HasPermission(AuthorizationHandlerContext context, DataOwnerRequirement requirement)
        {
            if (requirement.Permissions.Count() < 1) return false;
            var role = context.User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Role);
            if (role == null) return false;
            return await _roleService.HasPermissionAsync(role.Value, requirement.Permissions);
        }
    }
}
