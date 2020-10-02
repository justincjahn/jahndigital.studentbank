using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using jahndigital.studentbank.server.Services;
using Microsoft.AspNetCore.Authorization;

namespace jahndigital.studentbank.server.Permissions
{
    /// <summary>
    /// Ensures that the user has the permission needed to access the resource.
    /// </summary>
    internal class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly IRoleService _roleService;

        public PermissionAuthorizationHandler(IRoleService roleService) => _roleService = roleService;

        /// <summary>
        /// Check the database to ensure the user has the correct permissions.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var role = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);
            if (role == null) return;

            var auth = await _roleService.HasPermissionAsync(role.Value, requirement.Permission);
            if (auth) context.Succeed(requirement);
        }
    }
}
