using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace jahndigital.studentbank.server.Permissions
{
    internal class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(params string[] permission)
        {
            Permission = permission;
        }

        public IEnumerable<string> Permission { get; }
    }
}
