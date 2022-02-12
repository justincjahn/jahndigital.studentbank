using System.Threading.Tasks;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.WebApi.Permissions.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace JahnDigital.StudentBank.WebApi.Permissions.Providers;

/// <summary>
///     Dynamically builds authorization policies for built-in privileges.
/// </summary>
/// <remarks>
///     <code>DataOwner</code> denotes that the owner of the data queried or modified must be the same as the
///     authenticated user or student.
///
///     <code>DataOwner&lt;...Privilege&gt;</code> denotes that in addition to the data owner, roles with the provided
///     privilege(s) are also authorized to access the data.
/// </remarks>
internal class DataOwnerPermissionProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
    
    public DataOwnerPermissionProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.Contains(Constants.AuthPolicy.DataOwner))
        {
            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }

        if (policyName.Contains('<'))
        {
            return GetPermissionPolicy(policyName) as Task<AuthorizationPolicy?>;
        }

        AuthorizationPolicyBuilder policy =
            new AuthorizationPolicyBuilder().AddRequirements(new DataOwnerRequirement());

        return Task.FromResult(policy.Build()) as Task<AuthorizationPolicy?>;
    }

    /// <summary>
    ///     Determine what roles may be allowed to access the resource in addition to the data owner.
    /// </summary>
    /// <param name="policyName"></param>
    /// <returns></returns>
    private Task<AuthorizationPolicy> GetPermissionPolicy(string policyName)
    {
        int start = policyName.IndexOf('<');
        int end = policyName.LastIndexOf('>');
        string[] roles = policyName.Substring(start + 1, end - start - 1).Split(',');

        AuthorizationPolicyBuilder policy = new AuthorizationPolicyBuilder()
            .AddRequirements(new DataOwnerRequirement(roles));

        return Task.FromResult(policy.Build());
    }
}
