using System.Threading.Tasks;
using JahnDigital.StudentBank.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace JahnDigital.StudentBank.WebApi.Permissions.Providers;

/// <summary>
///     An <see cref="IAuthorizationPolicyProvider" /> that aggregates several to enable multiple different policy
///     providers to generate an <see cref="AuthorizationPolicy" />.
/// </summary>
public class AggregatePolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DataOwnerPermissionProvider _doPermissionProvider;

    private readonly PermissionPolicyProvider _permissionProvider;

    private readonly PreauthorizationProvider _preauthorizationProvider;

    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public AggregatePolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        _doPermissionProvider = new DataOwnerPermissionProvider(options);
        _permissionProvider = new PermissionPolicyProvider(options);
        _preauthorizationProvider = new PreauthorizationProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName) => GetPolicyProvider(policyName).GetPolicyAsync(policyName);

    /// <summary>
    ///     Fetch the appropriate <see cref="IAuthorizationPolicyProvider" /> for the policy string.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private IAuthorizationPolicyProvider GetPolicyProvider(string name)
    {
        if (name == Constants.AuthPolicy.Preauthorization)
        {
            return _preauthorizationProvider;
        }

        if (name.StartsWith(Constants.AuthPolicy.DataOwner))
        {
            return _doPermissionProvider;
        }

        return _permissionProvider;
    }
}
