using System.Linq;
using System.Security.Claims;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Execution;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.server.Permissions;
using Microsoft.AspNetCore.Http;

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
        /// <param name="contextAccessor"></param>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseSelection]
        public IQueryable<dal.Entities.Student> GetCurrentStudent(
            [Service] IHttpContextAccessor contextAccessor,
            [Service] AppDbContext context,
            [Service] IResolverContext resolverContext)
        {
            var id = contextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            var type = contextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == Constants.Auth.CLAIM_USER_TYPE);

            if (id == null) {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("User not found or not logged in.")
                        .SetCode("NOT_FOUND")
                        .Build()
                );
            }

            if (type.Value != Constants.UserType.Student.Name) {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("User not found or not logged in.")
                        .SetCode("NOT_FOUND")
                        .Build()
                );
            }

            resolverContext.ScopedContextData = resolverContext.ScopedContextData.SetItem(
                DataOwnerAuthorizationHandlerGraphQL.CTX_ISOWNER, true);

            return context.Students.Where(x => x.Id == int.Parse(id.Value));
        }

        /// <summary>
        /// Fetch information about a specific student.
        /// </summary>
        /// <param name="studentId">The ID number of the student to fetch.</param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.AuthPolicy.DataOwner + "<" + Constants.Privilege.PRIVILEGE_MANAGE_STUDENTS + ">")]
        [UseSelection]
        public IQueryable<dal.Entities.Student> GetStudent(long studentId, [Service] AppDbContext context) =>
            context.Students.Where(x => x.Id == studentId);
    }
}
