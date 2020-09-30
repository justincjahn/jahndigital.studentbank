using System.Collections.Generic;
using System.Threading.Tasks;

namespace jahndigital.studentbank.server.Services
{
    public interface IRoleService
    {
        /// <summary>
        /// Determine if the provided role has a given permission.
        /// </summary>
        /// <remarks>
        /// The <see cname="IDbInitializer" /> object seeds the database with built-in roles,
        /// so they are queryable alongsize custom roles.
        /// </remarks>
        /// <param name="role">The name of the role.</param>
        /// <param name="permission">The name of the privilege. <see cref="Constants.Privilege" /></param>
        /// <returns>True if the provided role has the provided permission.</returns>
        bool HasPermission(string role, string permission);

        /// <summary>
        /// Determine if the provided role has any of the given permissions.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        bool HasPermission(string role, IEnumerable<string> permissions);

        /// <summary>
        /// Determine if the provided role has a given permission.
        /// </summary>
        /// <remarks>
        /// The <see cname="IDbInitializer" /> object seeds the database with built-in roles,
        /// so they are queryable alongsize custom roles.
        /// </remarks>
        /// <param name="role">The name of the role.</param>
        /// <param name="permission">The name of the privilege. <see cref="Constants.Privilege" /></param>
        /// <returns>True if the provided role has the provided permission.</returns>
        Task<bool> HasPermissionAsync(string role, string permission);

        /// <summary>
        /// Determine if the provided role has any of the given permissions.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        Task<bool> HasPermissionAsync(string role, IEnumerable<string> permissions);
    }
}
