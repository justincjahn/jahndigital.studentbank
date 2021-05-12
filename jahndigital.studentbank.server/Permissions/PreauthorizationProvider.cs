using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace jahndigital.studentbank.server.Permissions
{
    /// <summary>
    /// Authorization policy provider requiring preauthorized users/students.
    /// </summary>
    internal class PreauthorizationProvider : IAuthorizationPolicyProvider
    {
        public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

        public PreauthorizationProvider(IOptions<AuthorizationOptions> options) =>
            FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
            FallbackPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
            FallbackPolicyProvider.GetFallbackPolicyAsync();

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            return Task.FromResult<AuthorizationPolicy?>(
                new AuthorizationPolicyBuilder()
                    .AddRequirements(new PreauthorizationRequirement())
                    .Build()
            );
        }
    }
}
