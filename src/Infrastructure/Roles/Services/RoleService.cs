using System.Collections.Concurrent;
using JahnDigital.StudentBank.Application.Roles.Services;
using JahnDigital.StudentBank.Domain.Entities;
using PrivilegeEnum = JahnDigital.StudentBank.Domain.Enums.Privilege;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Infrastructure.Roles.Services;

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

    /// <summary>
    ///     Factory to grab DbContext objects.
    /// </summary>
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
    public async Task<bool> HasPermissionAsync(string role, string permission, CancellationToken cancellationToken = new())
    {
        await using AppDbContext? context = await _factory.CreateDbContextAsync(cancellationToken: cancellationToken);

        if (!_cache.ContainsKey(role))
        {
            _cache.TryAdd(role, new ConcurrentDictionary<string, bool>());
        }

        if (!_cache[role].ContainsKey(permission))
        {
            long? roleId = await GetRoleId(role, context, cancellationToken);

            if (!roleId.HasValue)
            {
                return false;
            }

            RolePrivilege? dbPermissions = await context.RolePrivileges
                .Where(x => x.RoleId == roleId
                    && (x.Privilege.Name == permission
                        || x.Privilege.Name == PrivilegeEnum.All.Name))
                .SingleOrDefaultAsync(cancellationToken: cancellationToken);

            _cache[role].TryAdd(permission, dbPermissions != null);
        }

        return _cache[role][permission];
    }

    /// <inheritdoc />
    public async Task<bool> HasPermissionAsync(string role, IEnumerable<string> permissions, CancellationToken cancellationToken = new())
    {
        await using AppDbContext? context = await _factory.CreateDbContextAsync(cancellationToken);

        if (!_cache.ContainsKey(role))
        {
            _cache.TryAdd(role, new ConcurrentDictionary<string, bool>());
        }

        bool cache = _cache[role].Any(x => permissions.Contains(x.Key) && x.Value);

        if (cache)
        {
            return true;
        }

        // If there are missing permissions, fetch them
        var missing = permissions.Where(x => !_cache[role].ContainsKey(x)).ToList();

        if (missing.Count > 0)
        {
            long? roleId = await GetRoleId(role, context, cancellationToken);

            if (!roleId.HasValue)
            {
                return false;
            }

            var dbPermissions = await context.RolePrivileges
                .Include(x => x.Privilege)
                .Where(x => x.RoleId == roleId
                    && (missing.Contains(x.Privilege.Name)
                        || x.Privilege.Name == PrivilegeEnum.All.Name))
                .ToListAsync(cancellationToken: cancellationToken);

            foreach (string? missingPrivilege in missing)
            {
                _cache[role].TryAdd(
                    missingPrivilege,
                    dbPermissions.Any(x =>
                        x.Privilege.Name == missingPrivilege
                        || x.Privilege.Name == PrivilegeEnum.PRIVILEGE_ALL
                    )
                );
            }

            return await HasPermissionAsync(role, permissions, cancellationToken);
        }

        return false;
    }

    /// <summary>
    /// </summary>
    /// <param name="role"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<long?> GetRoleId(string role, AppDbContext context, CancellationToken cancellationToken)
    {
        if (!_roleCache.ContainsKey(role))
        {
            Role? dbRole = await context.Roles.SingleOrDefaultAsync(x => x.Name == role, cancellationToken: cancellationToken);

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
