using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Execution;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.server.Models;
using jahndigital.studentbank.services.DTOs;
using jahndigital.studentbank.services.Interfaces;
using jahndigital.studentbank.utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using static jahndigital.studentbank.utils.Constants;

namespace jahndigital.studentbank.server.GraphQL.Mutations
{
    /// <summary>
    ///     CRUD operations for <see cref="dal.Entities.Student" /> entities.
    /// </summary>
    [ExtendObjectType(Name = "Mutation")]
    public class StudentMutations : TokenManagerAbstract
    {
        /// <summary>
        ///     Log the student in using a username and password and return JWT tokens.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <param name="studentService"></param>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        public async Task<AuthenticateResponse> StudentLoginAsync(
            AuthenticateRequest input,
            [Service] AppDbContext context,
            [Service] IStudentService studentService,
            [Service] IHttpContextAccessor contextAccessor
        )
        {
            if (string.IsNullOrEmpty(input.Username)) {
                throw ErrorFactory.Unauthorized();
            }

            if (string.IsNullOrEmpty(input.Password)) {
                throw ErrorFactory.Unauthorized();
            }

            try {
                var response = await studentService.AuthenticateAsync(input, GetIp(contextAccessor));

                if (response == null) {
                    throw new QueryException(
                        ErrorBuilder.New()
                            .SetMessage("Bad username or password.")
                            .SetCode("LOGIN_FAIL")
                            .Build()
                    );
                }

                SetTokenCookie(contextAccessor, response.RefreshToken);

                return response;
            } catch (QueryException) {
                throw;
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }
        }

        /// <summary>
        ///     Obtain a new JWT token using a refresh token.
        /// </summary>
        /// <param name="token">The refresh token to use when obtaining a new JWT token. Must be valid and not expired.</param>
        /// <param name="context"></param>
        /// <param name="studentService"></param>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        public async Task<AuthenticateResponse> StudentRefreshTokenAsync(
            string? token,
            [Service] AppDbContext context,
            [Service] IStudentService studentService,
            [Service] IHttpContextAccessor contextAccessor
        )
        {
            token = token
                ?? GetToken(contextAccessor)
                ?? throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("A token is required.")
                        .SetCode(ErrorStrings.INVALID_REFRESH_TOKEN)
                        .Build());

            try {
                var response = await studentService.RefreshTokenAsync(token, GetIp(contextAccessor));

                if (response == null) {
                    throw new QueryException(
                        ErrorBuilder.New()
                            .SetMessage("Invalid refresh token.")
                            .SetCode(ErrorStrings.INVALID_REFRESH_TOKEN)
                            .Build()
                    );
                }

                SetTokenCookie(contextAccessor, response.RefreshToken);

                return response;
            } catch (QueryException) {
                throw;
            } catch (Exception e) {
                throw new QueryException(e.Message);
            }
        }

        /// <summary>
        ///     Revoke a refresh token.
        /// </summary>
        /// <param name="token">The refresh token to revoke.</param>
        /// <param name="context"></param>
        /// <param name="studentService"></param>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<bool> StudentRevokeRefreshTokenAsync(
            string? token,
            [Service] AppDbContext context,
            [Service] IStudentService studentService,
            [Service] IHttpContextAccessor contextAccessor
        )
        {
            token = token
                ?? GetToken(contextAccessor)
                ?? throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("A token is required.")
                        .SetCode(ErrorStrings.INVALID_REFRESH_TOKEN)
                        .Build());

            try {
                var response = await studentService.RevokeTokenAsync(token, GetIp(contextAccessor));

                if (!response) {
                    throw new QueryException(
                        ErrorBuilder.New()
                            .SetMessage("Token not found.")
                            .SetCode(ErrorStrings.ERROR_NOT_FOUND)
                            .Build()
                    );
                }

                ClearTokenCookie(contextAccessor);

                return response;
            } catch (QueryException) {
                throw;
            } catch (Exception e) {
                throw new QueryException(e.Message);
            }
        }

        /// <summary>
        ///     Attempt to generate a preauthorization token from the provided input.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="studentService"></param>
        /// <returns>A temporary JWT token if preauthorization is successful.</returns>
        public async Task<string> StudentPreregistrationAsync(
            StudentPreauthenticationRequest input,
            [Service] IStudentService studentService
        )
        {
            try {
                var response = await studentService.AuthenticateInviteAsync(input.InviteCode, input.AccountNumber);

                return response;
            } catch (ArgumentException) {
                throw ErrorFactory.Unauthorized();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }
        }

        /// <summary>
        ///     Register a student using the preauthorization token and provided input.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <param name="httpContext"></param>
        /// <returns>True if registration is successful, otherwise an error message.</returns>
        [Authorize(Policy = AuthPolicy.Preauthorization)]
        public async Task<bool> StudentRegistrationAsync(
            StudentRegisterRequest input,
            [Service] AppDbContext context,
            [Service] IHttpContextAccessor httpContext
        )
        {
            if (httpContext.HttpContext is null) {
                throw ErrorFactory.QueryFailed("Unable to obtain HttpContext.");
            }

            var userIdClaim = httpContext.HttpContext.User.Claims
                    .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)
                ?? throw ErrorFactory.Unauthorized();

            int userId;

            if (!int.TryParse(userIdClaim.Value, out userId)) {
                throw ErrorFactory.Unauthorized();
            }

            var userTypeClaim = httpContext.HttpContext.User.Claims
                    .FirstOrDefault(x => x.Type == Auth.CLAIM_USER_TYPE)
                ?? throw ErrorFactory.Unauthorized();

            if (userTypeClaim.Value != UserType.Student.Name) {
                throw ErrorFactory.Unauthorized();
            }

            var student = await context.Students
                    .FirstOrDefaultAsync(x => x.Id == userId)
                ?? throw ErrorFactory.NotFound();

            if (student.DateRegistered is not null) {
                throw ErrorFactory.Unauthorized();
            }

            var emailExists = await context.Instances
                .Where(x => x.IsActive)
                .Where(x => x.Groups.Any(g => g.Students.Any(s => s.Email == input.Email && s.Id != userId)))
                .AnyAsync();

            if (emailExists) {
                throw ErrorFactory.QueryFailed("A student with that email address already exists.");
            }

            student.Password = input.Password;
            student.DateRegistered = DateTime.UtcNow;
            student.Email = input.Email;

            try {
                await context.SaveChangesAsync();
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return true;
        }

        /// <summary>
        ///     Update an existing student.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseSelection, Authorize]
        public async Task<IQueryable<Student>> UpdateStudentAsync(
            UpdateStudentRequest input,
            [Service] AppDbContext context,
            [Service] IResolverContext resolverContext
        )
        {
            resolverContext.SetUser(input.Id, UserType.Student);
            var auth = await resolverContext.AuthorizeAsync(
                $"{AuthPolicy.DataOwner}<{Constants.Privilege.ManageStudents}>"
            );

            if (!auth.Succeeded) {
                throw ErrorFactory.Unauthorized();
            }

            Student student = await context.Students
                .Where(x => x.Id == input.Id)
                .SingleOrDefaultAsync();

            if (student == null) {
                throw ErrorFactory.NotFound();
            }

            if (input.GroupId != null) {
                var type = resolverContext.GetUserType() ?? throw ErrorFactory.Unauthorized();

                if (type != UserType.User) {
                    throw ErrorFactory.Unauthorized();
                }
            }

            _updateStudent(input, student);

            try {
                await context.SaveChangesAsync();
            } catch (DbUpdateException e) {
                throw ErrorFactory.QueryFailed(e.InnerException?.Message ?? e.Message);
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.ToString());
            }

            return context.Students.Where(x => x.Id == input.Id);
        }

        /// <summary>
        ///     Update a group of students at once.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseSelection, Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STUDENTS)]
        public async Task<IQueryable<Student>> UpdateBulkStudentAsync(
            IEnumerable<UpdateStudentRequest> input,
            [Service] AppDbContext context
        )
        {
            var ids = input.Select(x => x.Id);

            if (ids == null) {
                throw ErrorFactory.NotFound();
            }

            var students = await context.Students.Where(x => ids.Contains(x.Id)).ToListAsync();

            if (students.Count() != ids.Count()) {
                throw ErrorFactory.NotFound();
            }

            foreach (var request in input) {
                var student = students.Find(x => x.Id == request.Id);

                if (student == null) {
                    throw ErrorFactory.NotFound();
                }

                _updateStudent(request, student);
            }

            try {
                await context.SaveChangesAsync();
            } catch (DbUpdateException e) {
                throw ErrorFactory.QueryFailed(e.InnerException?.Message ?? e.Message);
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Students.Where(x => ids.Contains(x.Id));
        }

        /// <summary>
        ///     Update a student entity from the request parameters.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="student"></param>
        private void _updateStudent(UpdateStudentRequest request, Student student)
        {
            student.AccountNumber = request.AccountNumber ?? student.AccountNumber;
            student.Email = request.Email ?? student.Email;
            student.FirstName = request.FirstName ?? student.FirstName;
            student.LastName = request.LastName ?? student.LastName;
            student.GroupId = request.GroupId ?? student.GroupId;

            if (request.Password != null) {
                student.Password = request.Password;
            }
        }

        /// <summary>
        ///     Create a new student.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseSelection, Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STUDENTS)]
        public async Task<IQueryable<Student>> NewStudentAsync(
            NewStudentRequest input,
            [Service] AppDbContext context
        )
        {
            var groupExists = await context.Groups.Where(x => x.Id == input.GroupId).AnyAsync();

            if (!groupExists) {
                throw ErrorFactory.QueryFailed($"Provided Group ID ({input.GroupId}) does not exist.");
            }

            var studentExists = await context.Students.Where(x =>
                x.GroupId == input.GroupId
                && EF.Functions.Like(x.AccountNumber, $"%{input.AccountNumber}")
            ).AnyAsync();

            if (studentExists) {
                throw ErrorFactory.QueryFailed(
                    $"Provided Account Number {input.AccountNumber} already exists in group {input.GroupId}."
                );
            }

            var student = new Student {
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
        ///     Delete a student.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STUDENTS)]
        public async Task<bool> DeleteStudentAsync(long id, [Service] AppDbContext context)
        {
            var student = await context.Students.FindAsync(id);

            if (student == null) {
                throw ErrorFactory.NotFound();
            }

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
        ///     Restore a deleted student.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseSelection, Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_STUDENTS)]
        public async Task<IQueryable<Student>> RestoreStudentAsync(long id, [Service] AppDbContext context)
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

            return context.Students.Where(x => x.Id == id);
        }
    }
}