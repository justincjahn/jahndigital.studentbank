using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Execution;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.server.Models;
using jahndigital.studentbank.server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using static jahndigital.studentbank.server.Constants;

namespace jahndigital.studentbank.server.GraphQL.Mutations
{
    /// <summary>
    /// CRUD operations for <see cref="dal.Entities.Student"/> entities.
    /// </summary>
    [ExtendObjectType(Name = "Mutation")]
    public class StudentMutations : TokenManagerAbstract
    {
        /// <summary>
        /// Log the student in using a username and password and return JWT tokens.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <param name="studentService"></param>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        public AuthenticateResponse StudentLogin(
            AuthenticateRequest input,
            [Service] AppDbContext context,
            [Service] IStudentService studentService,
            [Service] IHttpContextAccessor contextAccessor
        ) {
            if (string.IsNullOrEmpty(input.Username)) throw ErrorFactory.Unauthorized();
            if (string.IsNullOrEmpty(input.Password)) throw ErrorFactory.Unauthorized();

            var response = studentService.Authenticate(input, GetIp(contextAccessor));

            if (response == null) {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("Bad username or password.")
                        .SetCode("LOGIN_FAIL")
                        .Build()
                );
            }

            SetTokenCookie(contextAccessor, response.RefreshToken);
            return response!;
        }

        /// <summary>
        /// Obtain a new JWT token using a refresh token.
        /// </summary>
        /// <param name="token">The refresh token to use when obtaining a new JWT token. Must be valid and not expired.</param>
        /// <param name="context"></param>
        /// <param name="studentService"></param>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        public AuthenticateResponse? StudentRefreshToken(
            string? token,
            [Service] AppDbContext context,
            [Service] IStudentService studentService,
            [Service] IHttpContextAccessor contextAccessor
        ) {
            token = token
                ?? GetToken(contextAccessor)
                ?? throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("A token is required.")
                        .SetCode(Constants.ErrorStrings.INVALID_REFRESH_TOKEN)
                        .Build());

            var response = studentService.RefreshToken(token, GetIp(contextAccessor));

            if (response == null) {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("Invalid refresh token.")
                        .SetCode(Constants.ErrorStrings.INVALID_REFRESH_TOKEN)
                        .Build()
                );
            }

            SetTokenCookie(contextAccessor, response.RefreshToken);
            return response;
        }

        /// <summary>
        /// Revoke a refresh token.
        /// </summary>
        /// <param name="token">The refresh token to revoke.</param>
        /// <param name="context"></param>
        /// <param name="studentService"></param>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        [Authorize]
        public bool StudentRevokeRefreshToken(
            string token,
            [Service] AppDbContext context,
            [Service] IStudentService studentService,
            [Service] IHttpContextAccessor contextAccessor
        ) {
            token = token
                ?? GetToken(contextAccessor)
                ?? throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("A token is required.")
                        .SetCode(Constants.ErrorStrings.INVALID_REFRESH_TOKEN)
                        .Build());

            var response = studentService.RevokeToken(token, GetIp(contextAccessor));

            if (!response) {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("Token not found.")
                        .SetCode("TOKEN_NOT_FOUND")
                        .Build()
                );
            }

            return response;
        }

        /// <summary>
        /// Update an existing student.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseSelection, Authorize]
        public async Task<IQueryable<dal.Entities.Student>> UpdateStudent(
            UpdateStudentRequest input,
            [Service] AppDbContext context,
            [Service] IResolverContext resolverContext
        ) {
            resolverContext.SetUser(input.Id, UserType.Student);
            var auth = await resolverContext.AuthorizeAsync(
                $"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManageStudents}>"
            );

            if (!auth.Succeeded) throw ErrorFactory.Unauthorized();

            dal.Entities.Student student = await context.Students
                .Where(x => x.Id == input.Id)
                .SingleOrDefaultAsync();
            
            if (student == null) throw ErrorFactory.NotFound();

            student.AccountNumber = input.AccountNumber ?? student.AccountNumber;
            student.Email = input.Email ?? student.Email;
            student.FirstName = input.FirstName ?? student.FirstName;
            student.LastName = input.LastName ?? student.LastName;

            if (input.GroupId != null) {
                var type = resolverContext.GetUserType() ?? throw ErrorFactory.Unauthorized();
                if (type != Constants.UserType.User) throw ErrorFactory.Unauthorized();
                student.GroupId = input.GroupId ?? student.GroupId;
            }

            if (input.Password != null) student.Password = input.Password;

            try {
                context.Update(student);
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Students.Where(x => x.Id == input.Id);
        }

        /// <summary>
        /// Create a new student.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STUDENTS)]
        public async Task<IQueryable<dal.Entities.Student>> NewStudent(
            NewStudentRequest input,
            [Service] AppDbContext context
        ) {
            var groupExists = await context.Groups.Where(x => x.Id == input.GroupId).AnyAsync();
            if (!groupExists) throw ErrorFactory.QueryFailed($"Provided Group ID ({input.GroupId}) does not exist.");

            var studentExists = await context.Students.Where(x =>
                x.GroupId == input.GroupId
                && EF.Functions.Like(x.AccountNumber, $"%{input.AccountNumber}")
            ).AnyAsync();

            if (studentExists) {
                throw ErrorFactory.QueryFailed(
                    $"Provided Account Number {input.AccountNumber} already exists in group {input.GroupId}."
                );
            }
            
            var student = new dal.Entities.Student {
                AccountNumber = input.AccountNumber,
                Email = input.Email,
                FirstName = input.FirstName,
                LastName = input.LastName,
                GroupId = input.GroupId,
                Password = input.Password
            };

            try {
                context.Add(student);
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.ToString());
            }

            return context.Students.Where(x => x.Id == student.Id);
        }
    
        /// <summary>
        /// Delete a student.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STUDENTS)]
        public async Task<bool> DeleteStudent(long id, [Service]AppDbContext context)
        {
            var student = await context.Students.FindAsync(id);
            if (student == null) throw ErrorFactory.NotFound();

            student.DateDeleted = DateTime.UtcNow;

            try {
                context.Update(student);
                await context.SaveChangesAsync();
            } catch {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Restore a deleted student.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STUDENTS)]
        public async Task<bool> RestoreStudent(long id, [Service]AppDbContext context)
        {
            var student = await context.Students
                .Where(x => x.Id == id && x.DateDeleted != null)
                .SingleOrDefaultAsync()
            ?? throw ErrorFactory.NotFound();

            student.DateDeleted = null;

            try {
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return true;
        }
    }
}
