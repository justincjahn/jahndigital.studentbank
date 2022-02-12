using System.Threading.Tasks;
using JahnDigital.StudentBank.WebApi.Permissions.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace JahnDigital.StudentBank.WebApi.Permissions.Providers;

/// <summary>
///     Authorization policy provider requiring preauthorized users/students.
/// </summary>
internal class PreauthorizationProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
    
    public PreauthorizationProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        return Task.FromResult<AuthorizationPolicy?>(
            new AuthorizationPolicyBuilder()
                .AddRequirements(new PreauthorizationRequirement())
                .Build()
        );
    }
}
