using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.server.Models;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.server.GraphQL.Mutations
{
    /// <summary>
    /// CRUD operations for <see cref="dal.Entities.Group"/> entities..
    /// </summary>
    [ExtendObjectType(Name = "Mutation")]
    public class GroupMutations
    {
        /// <summary>
        /// Update a <see cref="dal.Entities.Group"/>.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseSelection, Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_GROUPS)]
        public async Task<IQueryable<dal.Entities.Group>> UpdateGroupAsync(
            UpdateGroupRequest input,
            [Service] AppDbContext context
        ) {
            var group = await context.Groups.Where(x => x.Id == input.Id).SingleOrDefaultAsync()
             ?? throw ErrorFactory.NotFound();

            if (input.InstanceId != null) {
                var instanceExists = await context.Instances.Where(x => x.Id == input.InstanceId).AnyAsync();
                if (!instanceExists) throw ErrorFactory.QueryFailed($"Instance with ID {input.InstanceId} not found.");
                group.InstanceId = input.InstanceId.Value;
            }

            if (input.Name != null) {
                var groupExists = await context.Groups
                    .Where(x => 
                        x.Name == input.Name
                        && x.Id != group.Id
                        && x.InstanceId == group.InstanceId)
                    .AnyAsync();

                if (groupExists) {
                    throw ErrorFactory.QueryFailed(
                        $"A group named {input.Name} already exists in instance {group.InstanceId}."
                    );
                }

                group.Name = input.Name;
            }

            try {
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Groups.Where(x => x.Id == group.Id);
        }

        /// <summary>
        /// Create a new <see cref="dal.Entities.Group"/>.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseSelection, Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_GROUPS)]
        public async Task<IQueryable<dal.Entities.Group>> NewGroupAsync(
            NewGroupRequest input,
            [Service] AppDbContext context
        ) {
            var groupExists = await context.Groups.Where(x =>
                x.Name == input.Name && x.InstanceId == input.InstanceId
            ).AnyAsync();

            if (groupExists) {
                throw ErrorFactory.QueryFailed(
                    $"A group named '{input.Name}' already exists in instance {input.InstanceId}."
                );
            }

            var instanceExists = await context.Instances.Where(x => x.Id == input.InstanceId).AnyAsync();
            if (!instanceExists) {
                throw ErrorFactory.QueryFailed(
                    $"An instance with the ID {input.InstanceId} does not exist."
                );
            }

            var group = new dal.Entities.Group {
                Name = input.Name,
                InstanceId = input.InstanceId
            };

            try {
                context.Add(group);
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Groups.Where(x => x.Id == group.Id);
        }

        /// <summary>
        /// Delete a <see cref="dal.Entities.Group"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_GROUPS)]
        public async Task<bool> DeleteGroupAsync(long id, [Service]AppDbContext context)
        {
            var hasStudents = await context.Students.Where(x => x.GroupId == id).AnyAsync();
            if (hasStudents) throw ErrorFactory.QueryFailed("Cannot delete a group that still has students!");

            var group = await context.Groups.FindAsync(id);
            if (group == null) throw ErrorFactory.NotFound();

            group.DateDeleted = DateTime.UtcNow;

            try {
                context.Update(group);
                await context.SaveChangesAsync();
            } catch {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Restore a soft-deleted <see cref="dal.Entities.Group"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_GROUPS)]
        public async Task<bool> RestoreGroupAsync(long id, [Service]AppDbContext context)
        {
            var group = await context.Groups
                .Where(x => x.Id == id && x.DateDeleted != null)
                .SingleOrDefaultAsync()
            ?? throw ErrorFactory.NotFound();

            group.DateDeleted = null;

            try {
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return true;
        }
    }
}
