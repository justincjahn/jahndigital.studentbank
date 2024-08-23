using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Application.Students.Queries.GetStudent;
using JahnDigital.StudentBank.Application.Students.Queries.GetStudents;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.WebApi.Extensions;
using JahnDigital.StudentBank.WebApi.GraphQL.Common;
using MediatR;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Queries
{
    /// <summary>
    /// </summary>
    [ExtendObjectType("Query")]
    public class StudentQueries : RequestBase
    {
        /// <summary>
        ///     Get the currently logged in student's information (if the user is a student).
        /// </summary>
        /// <param name="resolverContext"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize]
        public async Task<IQueryable<Student>> GetCurrentStudentAsync(
            [SchemaService] IResolverContext resolverContext,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            if (resolverContext.GetUserType() != UserType.Student) throw ErrorFactory.NotFound();
            resolverContext.SetAuthorized(); // @note This shouldn't be necessary but the currentStudent.gql is pulling in groups.
            return await mediatr.Send(new GetStudentQuery(resolverContext.GetUserId()), cancellationToken);
        }

        /// <summary>
        ///     Fetch information about a specific student.
        /// </summary>
        /// <param name="studentId">The ID number of the student to fetch.</param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection,
         Authorize(Policy = Constants.AuthPolicy.DataOwner + "<" + Privilege.PRIVILEGE_MANAGE_STUDENTS + ">")]
        public async Task<IQueryable<Student>> GetStudentAsync(
            long studentId,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            return await mediatr.Send(new GetStudentQuery(studentId), cancellationToken);
        }

        /// <summary>
        ///     Get all students matching the criteria.
        /// </summary>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UsePaging, UseProjection, UseFiltering, UseSorting, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_STUDENTS)]
        public async Task<IQueryable<Student>> GetStudentsAsync(
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            return await mediatr.Send(new GetStudentsQuery(), cancellationToken);
        }

        /// <summary>
        ///     Get all deleted students matching criteria.
        /// </summary>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UsePaging, UseProjection, UseFiltering, UseSorting, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_STUDENTS)]
        public async Task<IQueryable<Student>> GetDeletedStudentsAsync(
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            return await mediatr.Send(new GetStudentsQuery(true), cancellationToken);
        }
    }
}
