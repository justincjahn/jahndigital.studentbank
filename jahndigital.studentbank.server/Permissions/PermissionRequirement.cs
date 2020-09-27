using Microsoft.AspNetCore.Authorization;

namespace jahndigital.studentbank.server.Permissions
{
    internal class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; private set; }

        public PermissionRequirement(string permission) => Permission = permission;
    }
}
