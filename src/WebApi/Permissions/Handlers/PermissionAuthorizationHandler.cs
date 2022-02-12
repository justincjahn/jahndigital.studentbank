using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JahnDigital.StudentBank.Application.Roles.Services;
using JahnDigital.StudentBank.WebApi.Permissions.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace JahnDigital.StudentBank.WebApi.Permissions.Handlers;

/// <summary>
///     Ensures that the user has the permission needed to access the resource.
/// </summary>
internal class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    /// <summary>
    /// </summary>
    private readonly IRoleService _roleService;

    public PermissionAuthorizationHandler(IRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>
    ///     Check the database to ensure the user has the correct permissions.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="requirement"></param>
    /// <returns></returns>
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Make sure the user isn't preauthenticated
        if (PreauthorizationHandler.AssertPreauthenticated(context))
        {
            return;
        }

        Claim? role = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);

        if (role == null)
        {
            return;
        }

        bool auth = await _roleService.HasPermissionAsync(role.Value, requirement.Permission);

        if (auth)
        {
            context.Succeed(requirement);
        }
    }
}
