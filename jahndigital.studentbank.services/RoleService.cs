using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.services.Interfaces;
using jahndigital.studentbank.utils;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.services
{
    /// <summary>
    /// Service that enables the validating role permissions.
    /// </summary>
    public class RoleService : IRoleService
    {
        /// <summary>
        /// Cache requests so we don't need to pull data from the database on every authentication iteration.
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, bool>> _cache = new Dictionary<string, Dictionary<string, bool>>();

        /// <summary>
        /// Cache for the roles.
        /// </summary>
        private readonly Dictionary<string, long> _roleCache = new Dictionary<string, long>();

        /// <summary>
        /// The database context to use when querying and updating the data store.
        /// </summary>
        private readonly AppDbContext _context;

        /// <summary>
        /// Initialize the instance with a database context.
        /// </summary>
        /// <param name="context"></param>
        public RoleService(AppDbContext context) => _context = context;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        private async Task<long?> GetRoleId(string role)
        {
            if (!this._roleCache.ContainsKey(role)) {
                var dbRole = await _context.Roles.SingleOrDefaultAsync(x => x.Name == role);
                if (dbRole == null) return null;
                this._roleCache.Add(role, dbRole.Id);
                return dbRole.Id;
            }

            return this._roleCache[role];
        }

        /// <inheritdoc />
        public async Task<bool> HasPermissionAsync(string role, string permission)
        {
            if (!this._cache.ContainsKey(role))  {
                this._cache.Add(role, new Dictionary<string, bool>());
            };

            if (!this._cache[role].ContainsKey(permission)) {
                var roleId = await GetRoleId(role);
                if (!roleId.HasValue) return false;

                var dbPermissions = await _context.RolePrivileges
                    .Where(x => x.RoleId == roleId
                        && (x.Privilege.Name == permission
                            || x.Privilege.Name == Constants.Privilege.All.Name))
                    .SingleOrDefaultAsync();
                
                this._cache[role].Add(permission, dbPermissions != null);
            }

            return this._cache[role][permission];
        }

        /// <inheritdoc />
        public async Task<bool> HasPermissionAsync(string role, IEnumerable<string> permissions)
        {
            if (!this._cache.ContainsKey(role))  {
                this._cache.Add(role, new Dictionary<string, bool>());
            };
            
            var cache = this._cache[role].Where(x => permissions.Contains(x.Key) && x.Value == true).Any();
            if (cache) return true;

            // If there are missing permissions, fetch them
            var missing = permissions.Where(x => !this._cache[role].Keys.Contains(x)).ToList();
            if (missing.Count > 0) {
                var roleId = await GetRoleId(role);
                if (!roleId.HasValue) return false;

                var dbPermissions = await _context.RolePrivileges
                    .Include(x => x.Privilege)
                    .Where(x => x.RoleId == roleId
                        && (missing.Contains(x.Privilege.Name)
                            || x.Privilege.Name == Constants.Privilege.All.Name))
                    .ToListAsync();
                
                foreach (var missingPrivilege in missing) {
                    if (
                        dbPermissions.Where(x =>
                            x.Privilege.Name == missingPrivilege
                            || x.Privilege.Name == Constants.Privilege.PRIVILEGE_ALL
                        ).Any()
                    ) {
                        this._cache[role].Add(missingPrivilege, true);
                    } else {
                        this._cache[role].Add(missingPrivilege, false);
                    }
                }

                return await HasPermissionAsync(role, permissions);
            }

            return false;
        }
    }
}
