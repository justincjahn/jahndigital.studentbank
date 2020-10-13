using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jahndigital.studentbank.dal.Contexts;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.server.Services
{
    /// <summary>
    /// Service that enables the validating role permissions.
    /// </summary>
    public class RoleService : IRoleService
    {
        /// <summary>
        /// The database context to use when querying and updating the data store.
        /// </summary>
        private readonly AppDbContext _context;

        /// <summary>
        /// Initialize the instance with a database context.
        /// </summary>
        /// <param name="context"></param>
        public RoleService(AppDbContext context) => _context = context;

        /// <inheritdoc />
        public async Task<bool> HasPermissionAsync(string role, string permission)
        {
            var dbRole = _context.Roles.SingleOrDefault(x => x.Name == role);
            if (dbRole == null) return false;

            var dbPermissions = await _context.RolePrivileges
                .Where(x => x.RoleId == dbRole.Id
                    && (x.Privilege.Name == permission
                        || x.Privilege.Name == Constants.Privilege.All.Name))
                .SingleOrDefaultAsync();

            return dbPermissions != null;
        }

        /// <inheritdoc />
        public async Task<bool> HasPermissionAsync(string role, IEnumerable<string> permissions)
        {
            var dbRole = await _context.Roles.SingleOrDefaultAsync(x => x.Name == role);
            if (dbRole == null) return false;

            var dbPermissions = await _context.RolePrivileges
                .Where(x => x.RoleId == dbRole.Id
                    && (permissions.Contains(x.Privilege.Name)
                        || x.Privilege.Name == Constants.Privilege.All.Name))
                .SingleOrDefaultAsync();
            
            return dbPermissions != null;
        }
    }
}
