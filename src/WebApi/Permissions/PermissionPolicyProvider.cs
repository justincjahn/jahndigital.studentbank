using System.Linq;
using System.Threading.Tasks;
using JahnDigital.StudentBank.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace JahnDigital.StudentBank.WebApi.Permissions
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
            if (Privilege.Privileges.SingleOrDefault(x => x.Name == policyName) != null)
            {
                AuthorizationPolicyBuilder? policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new PermissionRequirement(policyName));

                return Task.FromResult(policy.Build()) as Task<AuthorizationPolicy?>;
            }

            return FallbackPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}