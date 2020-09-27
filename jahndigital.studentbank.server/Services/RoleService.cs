using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.server.Services
{
    /// <summary>
    /// Service that enables the validating role permissions.
    /// </summary>
    public class RoleService : IRoleService
    {
        private readonly AppDbContext _context;

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
        public bool HasPermission(string role, string permission) => HasPermissionAsync(role, permission).Result;
    }
}
