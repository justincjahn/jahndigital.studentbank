using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace jahndigital.studentbank.server.Permissions
{
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
        public IEnumerable<string> Permissions { get; } = new string[] { };
    }
}