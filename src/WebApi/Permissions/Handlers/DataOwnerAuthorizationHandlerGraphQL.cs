using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using JahnDigital.StudentBank.Application.Roles.Services;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.WebApi.Extensions;
using JahnDigital.StudentBank.WebApi.Permissions.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace JahnDigital.StudentBank.WebApi.Permissions.Handlers;

/// <summary>
///     Validates that the user id contained in the request URL matches the authenticated user, asserting that the
///     data being requested is that of the authenticated user for GraphQL requests.
/// </summary>
internal class DataOwnerAuthorizationHandlerGraphQL : AuthorizationHandler<DataOwnerRequirement, IResolverContext>
{
    public const string CTX_ISOWNER = "IsDataOwner";

    public const string CTX_USER_ID = "UserId";

    public const string CTX_USER_TYPE = "UserType";

    private readonly IRoleService _roleService;

    public DataOwnerAuthorizationHandlerGraphQL(IRoleService roleService)
    {
        _roleService = roleService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext handlerContext,
        DataOwnerRequirement requirement,
        IResolverContext resolverContext
    )
    {
        if (PreauthorizationHandler.AssertPreauthenticated(handlerContext)) return;

        if (IsDataOwner(handlerContext, resolverContext))
        {
            handlerContext.Succeed(requirement);
            return;
        }

        if (await HasPermissionAsync(resolverContext, requirement)) handlerContext.Succeed(requirement);
    }

    /// <summary>
    ///     Determine if the current user is the owner of the resource.
    /// </summary>
    /// <remarks>
    ///     By convention the a request param called 'userId' should be used for user resources and 'studentId' should
    ///     be used for student resources.  The HotChocolate ScopedContext will be used if the values are set, then
    ///     the HotChocolate GlobalContext will be used.
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

        resolverContext.ScopedContextData = resolverContext.ScopedContextData.SetItem(CTX_ISOWNER, false);

        IReadOnlyList<ArgumentNode> route = resolverContext.Selection.SyntaxNode.Arguments;
        string userId = string.Empty;
        UserType? userType = null;

        // Check to see if the caller has specified the user ID and type of the data owner
        if (resolverContext.ScopedContextData.ContainsKey(CTX_USER_ID)
            && resolverContext.ScopedContextData.ContainsKey(CTX_USER_TYPE))
        {
            userId = resolverContext.ScopedContextData[CTX_USER_ID]?.ToString() ?? "";
            userType = (UserType)resolverContext.ScopedContextData[CTX_USER_TYPE]!;
        }
        else
        {
            // Try to pull the data owner from the GraphQL request
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

        // If no data owner was identified, then short circuit
        if (userType == null) return false;
        if (string.IsNullOrWhiteSpace(userId))  return false;

        var claimUserId = context.User.GetUserId();
        var claimUserType = context.User.GetUserType();

        // If the logged in user is anonymous, short circuit
        if (claimUserId == -1) return false;

        // If the userId is malformed, short circuit
        if (!int.TryParse(userId, out int routeId)) return false;

        // Assert the claim type matches the user type from the URL convention
        if (userType != claimUserType) return false;
        if (routeId != claimUserId) return false;

        // Cache for subsequent requests
        resolverContext.ScopedContextData = resolverContext.ScopedContextData.SetItem(CTX_ISOWNER, true);

        return true;
    }

    /// <summary>
    ///     Determine if the current user has permissions to access the resource.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="requirement"></param>
    /// <returns></returns>
    private async Task<bool> HasPermissionAsync(IResolverContext context, DataOwnerRequirement requirement)
    {
        if (!requirement.Permissions.Any()) return false;
        return await _roleService.HasPermissionAsync(context.GetUserRole().Name, requirement.Permissions);
    }
}
