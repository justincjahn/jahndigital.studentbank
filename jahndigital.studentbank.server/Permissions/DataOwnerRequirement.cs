using Microsoft.AspNetCore.Authorization;

namespace jahndigital.studentbank.server.Permissions
{
    /// <summary>
    /// Require that the user accessing the resource owns the data.
    /// </summary>
    public class DataOwnerRequirement : IAuthorizationRequirement { }
}
