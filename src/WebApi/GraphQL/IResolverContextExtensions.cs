using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.WebApi.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace JahnDigital.StudentBank.WebApi.GraphQL
{
    public static class IResolverContextExtensions
    {
        /// <summary>
        ///     Declaratively authorize the active user (if logged in) against the policy provided.
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
        ///     Explicitly mark the request as authorized.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="authorized"></param>
        public static IResolverContext SetAuthorized(this IResolverContext context, bool authorized = true)
        {
            context.ScopedContextData = context.ScopedContextData
                .SetItem(DataOwnerAuthorizationHandlerGraphQL.CTX_ISOWNER, authorized);

            return context;
        }

        /// <summary>
        ///     Get the <see cref="UserType" /> of the user currently logged in, if any.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static UserType? GetUserType(this IResolverContext context)
        {
            Claim? type = context.GetHttpContext().User.Claims
                .FirstOrDefault(x => x.Type == Constants.Auth.CLAIM_USER_TYPE);

            if (type != null)
            {
                return (UserType?)type.Value;
            }

            return null;
        }

        /// <summary>
        ///     Get the ID of the user currently logged in, if any.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static long? GetUserId(this IResolverContext context)
        {
            Claim? id = context.GetHttpContext().User.Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

            if (id != null)
            {
                return long.Parse(id.Value);
            }

            return null;
        }

        /// <summary>
        ///     Set the user ID and type for the appropriate authentication to occur.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        public static IResolverContext SetUser(this IResolverContext context, long userId, UserType type)
        {
            context.ScopedContextData = context.ScopedContextData.SetItems(new Dictionary<string, object?>
            {
                { DataOwnerAuthorizationHandlerGraphQL.CTX_USER_ID, userId },
                { DataOwnerAuthorizationHandlerGraphQL.CTX_USER_TYPE, type }
            });

            return context;
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
}