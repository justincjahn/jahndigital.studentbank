using System.Linq;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.server.Permissions;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    /// <summary>
    /// 
    /// </summary>
    [ExtendObjectType(Name = "Query")]
    public class StudentQueries
    {
        /// <summary>
        /// Get the currently logged in student's information (if the user is a student).
        /// </summary>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseSelection]
        public IQueryable<dal.Entities.Student> GetCurrentStudent(
            [Service] AppDbContext context,
            [Service] IResolverContext resolverContext
        ) {
            var id = resolverContext.GetUserId() ?? throw ErrorFactory.NotFound();
            var type = resolverContext.GetUserType() ?? throw ErrorFactory.NotFound();
            if (type != Constants.UserType.Student) throw ErrorFactory.NotFound();
            resolverContext.SetUser(id, type);

            resolverContext.ScopedContextData = resolverContext.ScopedContextData.SetItem(
                DataOwnerAuthorizationHandlerGraphQL.CTX_ISOWNER, true);

            return context.Students.Where(x => x.Id == id && x.DateDeleted == null);
        }

        /// <summary>
        /// Fetch information about a specific student.
        /// </summary>
        /// <param name="studentId">The ID number of the student to fetch.</param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseSelection,
        Authorize(Policy = Constants.AuthPolicy.DataOwner + "<" + Constants.Privilege.PRIVILEGE_MANAGE_STUDENTS + ">")]
        public IQueryable<dal.Entities.Student> GetStudent(long studentId, [Service]AppDbContext context) =>
            context.Students.Where(x => x.Id == studentId && x.DateDeleted == null);

        /// <summary>
        /// Get all students matching the criteria.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UsePaging, UseSelection, UseSorting, UseFiltering,
        Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STUDENTS)]
        public IQueryable<dal.Entities.Student> GetStudents([Service]AppDbContext context) =>
            context.Students.Where(x => x.DateDeleted == null);

        /// <summary>
        /// Get all deleted students matching criteria.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UsePaging, UseSelection, UseSorting, UseFiltering,
        Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STUDENTS)]
        public IQueryable<dal.Entities.Student> GetDeletedStudents([Service]AppDbContext context) =>
            context.Students.Where(x => x.DateDeleted != null);
    }
}
