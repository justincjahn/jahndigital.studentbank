using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace JahnDigital.StudentBank.WebApi.Permissions.Requirements;

/// <summary>
///     Require that the user accessing the resource owns the data.
/// </summary>
public class DataOwnerRequirement : IAuthorizationRequirement
{
    public DataOwnerRequirement() { }

    public DataOwnerRequirement(params string[] permissions)
    {
        Permissions = permissions;
    }

    /// <summary>
    /// </summary>
    public IEnumerable<string> Permissions { get; } = System.Array.Empty<string>();
}
