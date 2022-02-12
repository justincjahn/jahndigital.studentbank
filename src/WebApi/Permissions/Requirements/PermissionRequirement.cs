using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace JahnDigital.StudentBank.WebApi.Permissions.Requirements;

internal class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(params string[] permission)
    {
        Permission = permission;
    }

    public IEnumerable<string> Permission { get; }
}
