using System.Security.Claims;
using System.Threading.Tasks;
using JahnDigital.StudentBank.Application.Common;
using Microsoft.AspNetCore.Authorization;

namespace jahndigital.studentbank.server.Permissions
{
    public class PreauthorizationHandler : AuthorizationHandler<PreauthorizationRequirement>
    {
        /// <summary>
        ///     Ensures that the provided <see cname="AuthorizationHandlerContext" /> has a user that
        ///     is preauthenticated.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool AssertPreauthenticated(AuthorizationHandlerContext context)
        {
            Claim? claim = context.User.FindFirst(claim => claim.Type == Constants.Auth.CLAIM_PREAUTH_TYPE);

            if (claim is null)
            {
                return false;
            }

            if (claim.Value == Constants.Auth.CLAIM_PREAUTH_NO)
            {
                return false;
            }

            return true;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PreauthorizationRequirement requirement
        )
        {
            if (AssertPreauthenticated(context))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
