using System.Linq;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using JahnDigital.StudentBank.WebApi.Extensions;
using JahnDigital.StudentBank.WebApi.Permissions;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Queries
{
    /// <summary>
    /// </summary>
    [ExtendObjectType("Query")]
    public class StudentQueries
    {
        /// <summary>
        ///     Get the currently logged in student's information (if the user is a student).
        /// </summary>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection, Authorize]
        public IQueryable<Student> GetCurrentStudent(
            [ScopedService] AppDbContext context,
            [Service] IResolverContext resolverContext
        )
        {
            if (resolverContext.GetUserType() != UserType.Student) throw ErrorFactory.NotFound();
            resolverContext.SetAuthorized();

            return context
                .Students
                .Where(x => x.Id == resolverContext.GetUserId() && x.DateDeleted == null);
        }

        /// <summary>
        ///     Fetch information about a specific student.
        /// </summary>
        /// <param name="studentId">The ID number of the student to fetch.</param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection,
         Authorize(Policy = Constants.AuthPolicy.DataOwner + "<" + Privilege.PRIVILEGE_MANAGE_STUDENTS + ">")]
        public IQueryable<Student> GetStudent(
            long studentId,
            [ScopedService] AppDbContext context
        )
        {
            return context
                .Students
                .Where(x => x.Id == studentId && x.DateDeleted == null);
        }

        /// <summary>
        ///     Get all students matching the criteria.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_STUDENTS)]
        public IQueryable<Student> GetStudents([ScopedService] AppDbContext context)
        {
            return context
                .Students
                .Where(x => x.DateDeleted == null);
        }

        /// <summary>
        ///     Get all deleted students matching criteria.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         Authorize(Policy = Privilege.PRIVILEGE_MANAGE_STUDENTS)]
        public IQueryable<Student> GetDeletedStudents([ScopedService] AppDbContext context)
        {
            return context
                .Students
                .Where(x => x.DateDeleted != null);
        }
    }
}
