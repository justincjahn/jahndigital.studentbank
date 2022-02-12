using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application;
using JahnDigital.StudentBank.Application.Common.Utils;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using JahnDigital.StudentBank.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Mutations
{
    /// <summary>
    ///     CRUD operations for <see cref="Instance" /> entities.
    /// </summary>
    [ExtendObjectType("Mutation")]
    public class InstanceMutations
    {
        /// <summary>
        ///     Update an <see cref="Instance" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)),
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_INSTANCES)]
        public async Task<IQueryable<Instance>> UpdateInstanceAsync(
            UpdateInstanceRequest input,
            [ScopedService] AppDbContext context
        )
        {
            Instance? instance = await context.Instances.FindAsync(input.Id)
                ?? throw ErrorFactory.NotFound();

            if (input.Description != null)
            {
                bool hasName =
                    await context.Instances.AnyAsync(x => x.Description == input.Description && x.Id != instance.Id);

                if (hasName)
                {
                    throw ErrorFactory.QueryFailed(
                        $"Instance with name '{input.Description}' already exists."
                    );
                }

                instance.Description = input.Description;
            }

            // Only one instance can be active at a time
            if (input.IsActive == true)
            {
                foreach (Instance? inst in await context.Instances.ToListAsync())
                {
                    inst.IsActive = false;
                    context.Update(inst);
                }

                instance.IsActive = true;
            }

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                throw ErrorFactory.QueryFailed(e.InnerException?.Message ?? e.Message);
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Instances.Where(x => x.Id == instance.Id);
        }

        /// <summary>
        ///     Create an <see cref="Instance" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)),
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_INSTANCES)]
        public async Task<Instance> NewInstanceAsync(
            NewInstanceRequest input,
            [ScopedService] AppDbContext context,
            [Service] IConfiguration configuration
        )
        {
            bool hasInstance = await context.Instances.AnyAsync(x => x.Description == input.Description);

            if (hasInstance)
            {
                throw ErrorFactory.QueryFailed(
                    $"Instance with name '{input.Description}' already exists."
                );
            }

            // Generate a unique invite code for this new instance
            string code;

            do
            {
                code = InviteCode.NewCode(configuration.Get<AppConfig>().InviteCodeLength);
            } while (await context.Instances.AnyAsync(x => x.InviteCode == code));

            Instance? instance = new Instance { Description = input.Description, InviteCode = code };

            try
            {
                context.Add(instance);
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return instance;
        }

        /// <summary>
        ///     Soft-delete an <see cref="Instance" />.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)),
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_INSTANCES)]
        public async Task<bool> DeleteInstanceAsync(
            long id,
            [ScopedService] AppDbContext context
        )
        {
            Instance? instance = await context.Instances.FindAsync(id);

            if (instance == null)
            {
                throw ErrorFactory.NotFound();
            }

            bool hasGroups = await context.Groups.AnyAsync(x => x.InstanceId == id && x.DateDeleted == null);

            if (hasGroups)
            {
                throw ErrorFactory.QueryFailed("Cannot delete an instance that still has groups!");
            }

            instance.DateDeleted = DateTime.UtcNow;

            try
            {
                context.Update(instance);
                await context.SaveChangesAsync();
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Restore a soft-deleted <see cref="Instance" />.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)),
         UseProjection, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_INSTANCES)]
        public async Task<IQueryable<Instance>> RestoreInstanceAsync(
            long id,
            [ScopedService] AppDbContext context
        )
        {
            Instance? instance = await context.Instances
                    .Where(x => x.Id == id && x.DateDeleted != null)
                    .SingleOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            instance.DateDeleted = null;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Instances.Where(x => x.Id == id);
        }
    }
}
