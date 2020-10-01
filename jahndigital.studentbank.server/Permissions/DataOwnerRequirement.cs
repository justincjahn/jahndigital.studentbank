using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace jahndigital.studentbank.server.Permissions
{
    /// <summary>
    /// Require that the user accessing the resource owns the data.
    /// </summary>
    public class DataOwnerRequirement : IAuthorizationRequirement {
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> Permissions { get; private set;} = new string[] {};

        public DataOwnerRequirement() {}

        public DataOwnerRequirement(params string[] permissions) => Permissions = permissions;
    }
}
