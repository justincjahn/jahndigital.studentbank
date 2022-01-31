using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.dal.Entities;
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
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> _cache = new();

        /// <summary>
        ///     Cache for the roles.
        /// </summary>
        private readonly ConcurrentDictionary<string, long> _roleCache = new();

        private readonly IDbContextFactory<AppDbContext> _factory;

        /// <summary>
        ///     Initialize the instance with a database context factory.
        /// </summary>
        /// <param name="factory"></param>
        public RoleService(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
        }

        /// <inheritdoc />
        public async Task<bool> HasPermissionAsync(string role, string permission)
        {
            await using AppDbContext? context = _factory.CreateDbContext();

            if (!_cache.ContainsKey(role))
            {
                _cache.TryAdd(role, new ConcurrentDictionary<string, bool>());
            }

            if (!_cache[role].ContainsKey(permission))
            {
                long? roleId = await GetRoleId(role, context);

                if (!roleId.HasValue)
                {
                    return false;
                }

                RolePrivilege? dbPermissions = await context.RolePrivileges
                    .Where(x => x.RoleId == roleId
                        && (x.Privilege.Name == permission
                            || x.Privilege.Name == Constants.Privilege.All.Name))
                    .SingleOrDefaultAsync();

                _cache[role].TryAdd(permission, dbPermissions != null);
            }

            return _cache[role][permission];
        }

        /// <inheritdoc />
        public async Task<bool> HasPermissionAsync(string role, IEnumerable<string> permissions)
        {
            await using AppDbContext? context = _factory.CreateDbContext();

            if (!_cache.ContainsKey(role))
            {
                _cache.TryAdd(role, new ConcurrentDictionary<string, bool>());
            }

            ;

            bool cache = _cache[role].Where(x => permissions.Contains(x.Key) && x.Value).Any();

            if (cache)
            {
                return true;
            }

            // If there are missing permissions, fetch them
            List<string>? missing = permissions.Where(x => !_cache[role].Keys.Contains(x)).ToList();

            if (missing.Count > 0)
            {
                long? roleId = await GetRoleId(role, context);

                if (!roleId.HasValue)
                {
                    return false;
                }

                List<RolePrivilege>? dbPermissions = await context.RolePrivileges
                    .Include(x => x.Privilege)
                    .Where(x => x.RoleId == roleId
                        && (missing.Contains(x.Privilege.Name)
                            || x.Privilege.Name == Constants.Privilege.All.Name))
                    .ToListAsync();

                foreach (string? missingPrivilege in missing)
                {
                    if (
                        dbPermissions.Where(x =>
                            x.Privilege.Name == missingPrivilege
                            || x.Privilege.Name == Constants.Privilege.PRIVILEGE_ALL
                        ).Any()
                    )
                    {
                        _cache[role].TryAdd(missingPrivilege, true);
                    }
                    else
                    {
                        _cache[role].TryAdd(missingPrivilege, false);
                    }
                }

                return await HasPermissionAsync(role, permissions);
            }

            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="role"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<long?> GetRoleId(string role, AppDbContext context)
        {
            if (!_roleCache.ContainsKey(role))
            {
                Role? dbRole = await context.Roles.SingleOrDefaultAsync(x => x.Name == role);

                if (dbRole == null)
                {
                    return null;
                }

                _roleCache.TryAdd(role, dbRole.Id);

                return dbRole.Id;
            }

            return _roleCache[role];
        }
    }
}
