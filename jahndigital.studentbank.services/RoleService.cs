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
    ///     Service that enables the validating role permissions.
    /// </summary>
    public class RoleService : IRoleService
    {
        /// <summary>
        ///     Cache requests so we don't need to pull data from the database on every authentication iteration.
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, bool>> _cache = new();

        /// <summary>
        ///     The database context to use when querying and updating the data store.
        /// </summary>
        private readonly AppDbContext _context;

        /// <summary>
        ///     Cache for the roles.
        /// </summary>
        private readonly Dictionary<string, long> _roleCache = new();

        /// <summary>
        ///     Initialize the instance with a database context.
        /// </summary>
        /// <param name="context"></param>
        public RoleService(AppDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<bool> HasPermissionAsync(string role, string permission)
        {
            if (!_cache.ContainsKey(role)) {
                _cache.Add(role, new Dictionary<string, bool>());
            }

            ;

            if (!_cache[role].ContainsKey(permission)) {
                var roleId = await GetRoleId(role);

                if (!roleId.HasValue) {
                    return false;
                }

                var dbPermissions = await _context.RolePrivileges
                    .Where(x => x.RoleId == roleId
                        && (x.Privilege.Name == permission
                            || x.Privilege.Name == Constants.Privilege.All.Name))
                    .SingleOrDefaultAsync();

                _cache[role].Add(permission, dbPermissions != null);
            }

            return _cache[role][permission];
        }

        /// <inheritdoc />
        public async Task<bool> HasPermissionAsync(string role, IEnumerable<string> permissions)
        {
            if (!_cache.ContainsKey(role)) {
                _cache.Add(role, new Dictionary<string, bool>());
            }

            ;

            var cache = _cache[role].Where(x => permissions.Contains(x.Key) && x.Value).Any();

            if (cache) {
                return true;
            }

            // If there are missing permissions, fetch them
            var missing = permissions.Where(x => !_cache[role].Keys.Contains(x)).ToList();

            if (missing.Count > 0) {
                var roleId = await GetRoleId(role);

                if (!roleId.HasValue) {
                    return false;
                }

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
                        _cache[role].Add(missingPrivilege, true);
                    } else {
                        _cache[role].Add(missingPrivilege, false);
                    }
                }

                return await HasPermissionAsync(role, permissions);
            }

            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        private async Task<long?> GetRoleId(string role)
        {
            if (!_roleCache.ContainsKey(role)) {
                var dbRole = await _context.Roles.SingleOrDefaultAsync(x => x.Name == role);

                if (dbRole == null) {
                    return null;
                }

                _roleCache.Add(role, dbRole.Id);

                return dbRole.Id;
            }

            return _roleCache[role];
        }
    }
}