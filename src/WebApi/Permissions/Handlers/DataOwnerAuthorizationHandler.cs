using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Application.Roles.Services;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.WebApi.Permissions.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace JahnDigital.StudentBank.WebApi.Permissions.Handlers;

/// <summary>
///     Validates that the user id contained in the request URL matches the authenticated user, asserting that the
///     data being requested is that of the authenticated user.
/// </summary>
internal class DataOwnerAuthorizationHandler : AuthorizationHandler<DataOwnerRequirement>
{
    private readonly IHttpContextAccessor _httpContext;
    private readonly IRoleService _roleService;

    public DataOwnerAuthorizationHandler(IHttpContextAccessor context, IRoleService roleService)
    {
        _httpContext = context;
        _roleService = roleService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        DataOwnerRequirement requirement)
    {
        if (PreauthorizationHandler.AssertPreauthenticated(context))
        {
            return;
        }

        if (IsDataOwner(context))
        {
            context.Succeed(requirement);
            return;
        }

        bool hasPermission = await HasPermission(context, requirement);

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
    /// <returns>true if the current user/student is a data owner.</returns>
    private bool IsDataOwner(AuthorizationHandlerContext context)
    {
        RouteValueDictionary route = _httpContext.HttpContext?.Request.RouteValues
            ?? throw new Exception("Unable to get HttpContext when calling IsDataOwner.");

        string userId = string.Empty;
        UserType? userType = null;

        if (route.ContainsKey("userId"))
        {
            userType = UserType.User;
            userId = route["userId"]?.ToString()!;
        }

        if (route.ContainsKey("studentId"))
        {
            userType = UserType.Student;
            userId = route["studentId"]?.ToString()!;
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

        Claim? userTypeClaim = context.User.Claims.FirstOrDefault(x => x.Type == Constants.Auth.CLAIM_USER_TYPE);

        if (userTypeClaim == null)
        {
            return false;
        }

        if (!int.TryParse(userId, out int routeId))
        {
            return false;
        }

        if (!int.TryParse(userIdClaim.Value, out int claimId))
        {
            return false;
        }

        // Assert the claim type matches the user type from the URL convention
        if (userType.Name != userTypeClaim.Value)
        {
            return false;
        }

        return routeId == claimId;
    }

    /// <summary>
    ///     Determine if the current user has permissions to access the resource.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="requirement"></param>
    /// <returns></returns>
    private async Task<bool> HasPermission(AuthorizationHandlerContext context, DataOwnerRequirement requirement)
    {
        if (!requirement.Permissions.Any())
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
