using System.Linq;
using System.Threading.Tasks;
using jahndigital.studentbank.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace jahndigital.studentbank.server.Permissions
{
    /// <summary>
    ///     Dynamically builds authorization policies for built-in privileges.
    /// </summary>
    internal class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
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
            if (Constants.Privilege.Privileges.SingleOrDefault(x => x.Name == policyName) != null) {
                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new PermissionRequirement(policyName));

                return Task.FromResult(policy.Build()) as Task<AuthorizationPolicy?>;
            }

            return FallbackPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}