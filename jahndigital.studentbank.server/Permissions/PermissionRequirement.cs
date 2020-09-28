using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace jahndigital.studentbank.server.Permissions
{
    internal class PermissionRequirement : IAuthorizationRequirement
    {
        public IEnumerable<string> Permission { get; private set; }

        public PermissionRequirement(params string[] permission) => Permission = permission;
    }
}
