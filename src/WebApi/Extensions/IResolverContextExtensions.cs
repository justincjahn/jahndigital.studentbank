using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HotChocolate.Execution;
using HotChocolate.Resolvers;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.WebApi.GraphQL;
using JahnDigital.StudentBank.WebApi.Permissions.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace JahnDigital.StudentBank.WebApi.Extensions;

public static class IResolverContextExtensions
{
    /// <summary>
    ///     Authorize the active user (if logged in) against the policy provided.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="policy"></param>
    /// <returns></returns>
    public static Task<AuthorizationResult> AuthorizeAsync(this IResolverContext context, string policy)
    {
        IAuthorizationService? svc = context.Service<IAuthorizationService>();
        ClaimsPrincipal? usr = context.GetHttpContext().User;
        return svc.AuthorizeAsync(usr, context, policy);
    }

    /// <summary>
    ///     Authorize the active user against the policy provided and throw an exception if unauthorized.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="policy"></param>
    /// <exception cref="QueryException"></exception>
    public static async Task AssertAuthorizedAsync(this IResolverContext context, string policy)
    {
        var result = await AuthorizeAsync(context, policy);
        if (!result.Succeeded) throw ErrorFactory.Unauthorized();
    }

    /// <summary>
    ///     Explicitly mark the request as authorized.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="authorized"></param>
    public static IResolverContext SetAuthorized(this IResolverContext context, bool authorized = true)
    {
        context.ScopedContextData = context
            .ScopedContextData
            .SetItem(DataOwnerAuthorizationHandlerGraphQL.CTX_ISOWNER, authorized);

        return context;
    }

    /// <summary>
    ///     Get the <see cref="UserType" /> of the user currently logged in, if any.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static UserType GetUserType(this IResolverContext context) => context.GetHttpContext().GetUserType();

    /// <summary>
    ///     Get the ID of the user currently logged in, if any.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static long GetUserId(this IResolverContext context) => context.GetHttpContext().GetUserId();

    /// <summary>
    ///     Get the role of the currently logged in role, if any.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static Role GetUserRole(this IResolverContext context) => context.GetHttpContext().GetUserRole();

    /// <summary>
    ///     Set a specific user/student ID and <see cref="UserType"/> that owns the data being requested for DataOwner
    ///     authorization.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="userId"></param>
    /// <param name="type"></param>
    public static IResolverContext SetDataOwner(this IResolverContext context, long userId, UserType type)
    {
        context.ScopedContextData = context.ScopedContextData.SetItems(new Dictionary<string, object?>
        {
            { DataOwnerAuthorizationHandlerGraphQL.CTX_USER_ID, userId },
            { DataOwnerAuthorizationHandlerGraphQL.CTX_USER_TYPE, type }
        });

        return context;
    }

    /// <summary>
    /// Set the data owner from the currently authenticated claim.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static IResolverContext SetDataOwner(this IResolverContext context)
    {
        return SetDataOwner(context, context.GetUserId(), context.GetUserType());
    }

    /// <summary>
    ///     Get the HTTP context from the resolver.
    /// </summary>
    /// <param name="context"></param>
    public static HttpContext GetHttpContext(this IResolverContext context)
    {
        (_, object? value) = context.ContextData.First(x => x.Value is HttpContext);

        if (value is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        return (HttpContext)value;
    }
}
