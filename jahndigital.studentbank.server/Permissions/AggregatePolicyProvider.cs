using System.Threading.Tasks;
using jahndigital.studentbank.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace jahndigital.studentbank.server.Permissions
{
    /// <summary>
    ///     An <see cref="IAuthorizationPolicyProvider" /> that aggregates several to enable multiple
    ///     different policy providers to generate an <see cref="AuthorizationPolicy" />.
    /// </summary>
    public class AggregatePolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DataOwnerPermissionProvider _doPermissionProvider;

        private readonly PermissionPolicyProvider _permissionProvider;

        private readonly PreauthorizationProvider _preauthorizationProvider;
        private IHttpContextAccessor _httpContext;

        public AggregatePolicyProvider(
            IHttpContextAccessor httpContextAccessor,
            IOptions<AuthorizationOptions> options
        )
        {
            FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
            _httpContext = httpContextAccessor;
            _doPermissionProvider = new DataOwnerPermissionProvider(options);
            _permissionProvider = new PermissionPolicyProvider(options);
            _preauthorizationProvider = new PreauthorizationProvider(options);
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

        /// <summary>
        /// </summary>
        /// <param name="policyName"></param>
        /// <returns></returns>
        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            return GetPolicyProvider(policyName).GetPolicyAsync(policyName);
        }

        /// <summary>
        ///     Fetch the appropriate <see cref="IAuthorizationPolicyProvider" /> for the
        ///     policy string.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private IAuthorizationPolicyProvider GetPolicyProvider(string name)
        {
            if (name == Constants.AuthPolicy.Preauthorization) {
                return _preauthorizationProvider;
            }

            if (name.StartsWith(Constants.AuthPolicy.DataOwner)) {
                return _doPermissionProvider;
            }

            return _permissionProvider;
        }
    }
}