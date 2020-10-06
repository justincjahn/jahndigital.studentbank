using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using jahndigital.studentbank.server.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using static jahndigital.studentbank.server.Constants;

namespace jahndigital.studentbank.server.GraphQL
{
    public static class IResolverContextExtensions
    {
        /// <summary>
        /// Declaratively authorize the active user (if logged in) against the policy provided.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        public static Task<AuthorizationResult> AuthorizeAsync(this IResolverContext context, string policy)
        {
            var svc = context.Service<IAuthorizationService>();
            var usr = context.GetHttpContext().User;
            return svc.AuthorizeAsync(usr, context, policy);
        }

        /// <summary>
        /// Explicitly mark the request as authorized.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="authorized"></param>
        public static IResolverContext SetAuthorized(this IResolverContext context, bool authorized = true) {
            context.ScopedContextData = context.ScopedContextData
                .SetItem(DataOwnerAuthorizationHandlerGraphQL.CTX_ISOWNER, authorized);
            
            return context;
        }
        
        /// <summary>
        /// Set the user ID and type for the appropriate authentication to occur.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        public static IResolverContext SetUser(this IResolverContext context, long userId, UserType type)
        {
            context.ScopedContextData = context.ScopedContextData.SetItems(new Dictionary<string, object> {
                {DataOwnerAuthorizationHandlerGraphQL.CTX_USER_ID, userId},
                {DataOwnerAuthorizationHandlerGraphQL.CTX_USER_TYPE, type}
            });

            return context;
        }

        /// <summary>
        /// Get the HTTP context from the resolver.
        /// </summary>
        /// <param name="context"></param>
        public static HttpContext GetHttpContext(this IResolverContext context)
        {
            var ctx = context.ContextData.FirstOrDefault(x => x.Value is HttpContext);
            return (HttpContext)ctx.Value;
        }
    }
}
