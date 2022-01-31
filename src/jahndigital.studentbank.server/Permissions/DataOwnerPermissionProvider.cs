using System.Threading.Tasks;
using jahndigital.studentbank.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace jahndigital.studentbank.server.Permissions
{
    /// <summary>
    ///     Dynamically builds authorization policies for built-in privileges.
    /// </summary>
    internal class DataOwnerPermissionProvider : IAuthorizationPolicyProvider
    {
        public DataOwnerPermissionProvider(IOptions<AuthorizationOptions> options)
        {
            FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return FallbackPolicyProvider.GetDefaultPolicyAsync();
        }

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return FallbackPolicyProvider.GetFallbackPolicyAsync();
        }

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (!policyName.Contains(Constants.AuthPolicy.DataOwner))
            {
                return FallbackPolicyProvider.GetPolicyAsync(policyName);
            }

            if (policyName.Contains('<'))
            {
                return GetPermissionPolicy(policyName) as Task<AuthorizationPolicy?>;
            }

            AuthorizationPolicyBuilder? policy =
                new AuthorizationPolicyBuilder().AddRequirements(new DataOwnerRequirement());

            return Task.FromResult(policy.Build()) as Task<AuthorizationPolicy?>;
        }

        /// <summary>
        ///     Determine what roles may be allowed to access the resource in addition to the data owner.
        /// </summary>
        /// <param name="policyName"></param>
        /// <returns></returns>
        public Task<AuthorizationPolicy> GetPermissionPolicy(string policyName)
        {
            int start = policyName.IndexOf('<');
            int end = policyName.LastIndexOf('>');
            string[]? roles = policyName.Substring(start + 1, end - start - 1).Split(',');
            AuthorizationPolicyBuilder? policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new DataOwnerRequirement(roles));

            return Task.FromResult(policy.Build());
        }
    }
}
