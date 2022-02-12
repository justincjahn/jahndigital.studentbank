using System.Linq;
using System.Threading.Tasks;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.WebApi.Permissions.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace JahnDigital.StudentBank.WebApi.Permissions.Providers;

/// <summary>
///     Dynamically builds authorization policies for built-in privileges.
/// </summary>
internal class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
    
    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (Privilege.Privileges.SingleOrDefault(x => x.Name == policyName) == null)
        {
            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }

        AuthorizationPolicyBuilder policy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName));

        return Task.FromResult(policy.Build()) as Task<AuthorizationPolicy?>;

    }
}
