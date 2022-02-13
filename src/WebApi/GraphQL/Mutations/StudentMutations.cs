using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Execution;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Application.Common.DTOs;
using JahnDigital.StudentBank.Application.Students.Commands.AdminUpdateStudent;
using JahnDigital.StudentBank.Application.Students.Commands.AuthenticateStudent;
using JahnDigital.StudentBank.Application.Students.Commands.AuthenticateStudentInvite;
using JahnDigital.StudentBank.Application.Students.Commands.RefreshStudentToken;
using JahnDigital.StudentBank.Application.Students.Commands.RegisterStudent;
using JahnDigital.StudentBank.Application.Students.Commands.RevokeStudentToken;
using JahnDigital.StudentBank.Application.Students.Commands.UpdateStudent;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using JahnDigital.StudentBank.WebApi.Extensions;
using JahnDigital.StudentBank.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Mutations
{
    /// <summary>
    ///     CRUD operations for <see cref="Student" /> entities.
    /// </summary>
    [ExtendObjectType("Mutation")]
    public class StudentMutations : TokenManagerAbstract
    {
        /// <summary>
        ///     Log the student in using a username and password and return JWT tokens.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <param name="mediatr"></param>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext))]
        public async Task<AuthenticateResponse> StudentLoginAsync(
            AuthenticateRequest input,
            [ScopedService] AppDbContext context,
            [Service] ISender mediatr,
            [Service] IHttpContextAccessor contextAccessor
        )
        {
            if (string.IsNullOrEmpty(input.Username))
            {
                throw ErrorFactory.Unauthorized();
            }

            if (string.IsNullOrEmpty(input.Password))
            {
                throw ErrorFactory.Unauthorized();
            }

            try
            {
                var command = new AuthenticateStudentCommand(input.Username, input.Password, GetIp(contextAccessor));
                var response = await mediatr.Send(command);
                SetTokenCookie(contextAccessor, response.RefreshToken);
                return response;
            }
            catch (Exception)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("Bad username or password.")
                        .SetCode("LOGIN_FAIL")
                        .Build()
                );
            }
        }

        /// <summary>
        ///     Obtain a new JWT token using a refresh token.
        /// </summary>
        /// <param name="token">The refresh token to use when obtaining a new JWT token. Must be valid and not expired.</param>
        /// <param name="context"></param>
        /// <param name="mediatr"></param>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext))]
        public async Task<AuthenticateResponse> StudentRefreshTokenAsync(
            string? token,
            [ScopedService] AppDbContext context,
            [Service] ISender mediatr,
            [Service] IHttpContextAccessor contextAccessor
        )
        {
            token = token
                ?? GetToken(contextAccessor)
                ?? throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("A token is required.")
                        .SetCode(Constants.ErrorStrings.INVALID_REFRESH_TOKEN)
                        .Build());

            try
            {
                var command = new RefreshStudentTokenCommand(token, GetIp(contextAccessor));
                var response = await mediatr.Send(command);
                SetTokenCookie(contextAccessor, response.RefreshToken);
                return response;
            }
            catch (Exception)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("Invalid refresh token.")
                        .SetCode(Constants.ErrorStrings.INVALID_REFRESH_TOKEN)
                        .Build()
                );
            }
        }

        /// <summary>
        ///     Revoke a refresh token.
        /// </summary>
        /// <param name="token">The refresh token to revoke.</param>
        /// <param name="mediatr"></param>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        [HotChocolate.AspNetCore.Authorization.Authorize]
        public async Task<bool> StudentRevokeRefreshTokenAsync(
            string? token,
            [Service] ISender mediatr,
            [Service] IHttpContextAccessor contextAccessor
        )
        {
            token = token
                ?? GetToken(contextAccessor)
                ?? throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("A token is required.")
                        .SetCode(Constants.ErrorStrings.INVALID_REFRESH_TOKEN)
                        .Build());

            try
            {
                var command = new RevokeStudentTokenCommand(token, GetIp(contextAccessor));
                await mediatr.Send(command);
                ClearTokenCookie(contextAccessor);
                return true;
            }
            catch (Exception)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("Token not found.")
                        .SetCode(Constants.ErrorStrings.ERROR_NOT_FOUND)
                        .Build()
                );
            }
        }

        /// <summary>
        ///     Attempt to generate a preauthorization token from the provided input.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <returns>A temporary JWT token if preauthorization is successful.</returns>
        public async Task<string> StudentPreregistrationAsync(
            StudentPreauthenticationRequest input,
            [Service] ISender mediatr
        )
        {
            try
            {
                var command = new AuthenticateStudentInviteCommand(input.InviteCode, input.AccountNumber);
                var response = await mediatr.Send(command);
                return response;
            }
            catch (Exception)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("Invalid invite code or account number")
                        .SetCode(Constants.ErrorStrings.ERROR_QUERY_FAILED)
                        .Build()
                );
            }
        }

        /// <summary>
        ///     Register a student using the preauthorization token and provided input.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <param name="httpContext"></param>
        /// <param name="mediatr"></param>
        /// <returns>True if registration is successful, otherwise an error message.</returns>
        [UseDbContext(typeof(AppDbContext)),
         HotChocolate.AspNetCore.Authorization.Authorize(Policy = Constants.AuthPolicy.Preauthorization)]
        public async Task<bool> StudentRegistrationAsync(
            StudentRegisterRequest input,
            [ScopedService] AppDbContext context,
            [Service] IHttpContextAccessor httpContext,
            [Service] ISender mediatr
        )
        {
            if (httpContext.HttpContext is null)
            {
                throw ErrorFactory.QueryFailed("Unable to obtain HttpContext.");
            }

            Claim? userIdClaim = httpContext.HttpContext.User.Claims
                    .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)
                ?? throw ErrorFactory.Unauthorized();

            int userId;

            if (!int.TryParse(userIdClaim.Value, out userId))
            {
                throw ErrorFactory.Unauthorized();
            }

            Claim? userTypeClaim = httpContext.HttpContext.User.Claims
                    .FirstOrDefault(x => x.Type == Constants.Auth.CLAIM_USER_TYPE)
                ?? throw ErrorFactory.Unauthorized();

            if (userTypeClaim.Value != UserType.Student.Name)
            {
                throw ErrorFactory.Unauthorized();
            }

            Student? student = await context.Students
                    .FirstOrDefaultAsync(x => x.Id == userId)
                ?? throw ErrorFactory.NotFound();

            if (student.DateRegistered is not null)
            {
                throw ErrorFactory.Unauthorized();
            }

            bool emailExists = await context.Instances
                .Where(x => x.IsActive)
                .Where(x => x.Groups.Any(g => g.Students.Any(s => s.Email == input.Email && s.Id != userId)))
                .AnyAsync();

            if (emailExists)
            {
                throw ErrorFactory.QueryFailed("A student with that email address already exists.");
            }

            var command = new RegisterStudentCommand(student.Id, DateTime.UtcNow, input.Password, input.Email);

            try
            {
                await mediatr.Send(command);
            }
            catch (Exception e)
            {
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
        /// <param name="mediatr"></param>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection, HotChocolate.AspNetCore.Authorization.Authorize]
        public async Task<IQueryable<Student>> UpdateStudentAsync(
            UpdateStudentRequest input,
            [ScopedService] AppDbContext context,
            [Service] IResolverContext resolverContext,
            [Service] ISender mediatr,
            [Service] IHttpContextAccessor contextAccessor
        )
        {
            await resolverContext
                .SetDataOwner(input.Id, UserType.Student)
                .AssertAuthorizedAsync($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageStudents}>");

            Student student = await context
                    .Students
                    .Where(x => x.Id == input.Id)
                    .SingleOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            if (input.GroupId != null)
            {
                if (resolverContext.GetUserType() != UserType.User) throw ErrorFactory.Unauthorized();
            }

            if (input.Password is not null && resolverContext.GetUserType() != UserType.User)
            {
                if (input.CurrentPassword is null)
                {
                    throw new QueryException(
                        ErrorBuilder
                            .New()
                            .SetMessage("Bad username or password.")
                            .SetCode("LOGIN_FAIL")
                            .Build()
                    );
                }

                var authCommand = new AuthenticateStudentCommand(
                    student.AccountNumber,
                    input.CurrentPassword,
                    GetIp(contextAccessor)
                );

                try
                {
                    await mediatr.Send(authCommand);
                }
                catch
                {
                    throw new QueryException(
                        ErrorBuilder.New()
                            .SetMessage("Bad username or password.")
                            .SetCode("LOGIN_FAIL")
                            .Build()
                    );
                }
            }

            var command = new UpdateStudentCommand(student.Id, input.Email, input.Password);

            try
            {
                await mediatr.Send(command);
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context
                .Students
                .Where(x => x.Id == input.Id);
        }

        /// <summary>
        ///     Update a group of students at once.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <param name="mediatr"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection,
         HotChocolate.AspNetCore.Authorization.Authorize(Policy = Privilege.PRIVILEGE_MANAGE_STUDENTS)]
        public async Task<IQueryable<Student>> UpdateBulkStudentAsync(
            IEnumerable<UpdateStudentRequest> input,
            [ScopedService] AppDbContext context,
            [ScopedService] ISender mediatr
        )
        {
            IEnumerable<long>? ids = input.Select(x => x.Id);

            if (ids == null)
            {
                throw ErrorFactory.NotFound();
            }

            List<Student>? students = await context.Students.Where(x => ids.Contains(x.Id)).ToListAsync();

            if (students.Count() != ids.Count())
            {
                throw ErrorFactory.NotFound();
            }

            foreach (UpdateStudentRequest? request in input)
            {
                Student? student = students.Find(x => x.Id == request.Id);

                if (student == null)
                {
                    throw ErrorFactory.NotFound();
                }

                var command = new AdminUpdateStudentCommand(student.Id, request.AccountNumber, request.Email,
                    request.FirstName, request.LastName, request.GroupId, request.Password);

                try
                {
                    await mediatr.Send(command);
                }
                catch (Exception e)
                {
                    throw ErrorFactory.QueryFailed(e.Message);
                }
            }

            return context.Students.Where(x => ids.Contains(x.Id));
        }

        /// <summary>
        ///     Create a new student.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection,
         HotChocolate.AspNetCore.Authorization.Authorize(Policy = Privilege.PRIVILEGE_MANAGE_STUDENTS)]
        public async Task<IQueryable<Student>> NewStudentAsync(
            NewStudentRequest input,
            [ScopedService] AppDbContext context
        )
        {
            bool groupExists = await context.Groups.Where(x => x.Id == input.GroupId).AnyAsync();

            if (!groupExists)
            {
                throw ErrorFactory.QueryFailed($"Provided Group ID ({input.GroupId}) does not exist.");
            }

            bool studentExists = await context.Students.Where(x =>
                x.GroupId == input.GroupId
                && EF.Functions.Like(x.AccountNumber, $"%{input.AccountNumber}")
            ).AnyAsync();

            if (studentExists)
            {
                throw ErrorFactory.QueryFailed(
                    $"Provided Account Number {input.AccountNumber} already exists in group {input.GroupId}."
                );
            }

            Student? student = new Student
            {
                AccountNumber = input.AccountNumber,
                Email = input.Email,
                FirstName = input.FirstName,
                LastName = input.LastName,
                GroupId = input.GroupId,
                Password = input.Password
            };

            try
            {
                context.Add(student);
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
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
        [UseDbContext(typeof(AppDbContext)),
         HotChocolate.AspNetCore.Authorization.Authorize(Policy = Privilege.PRIVILEGE_MANAGE_STUDENTS)]
        public async Task<bool> DeleteStudentAsync(
            long id,
            [ScopedService] AppDbContext context
        )
        {
            Student? student = await context.Students.FindAsync(id);

            if (student == null)
            {
                throw ErrorFactory.NotFound();
            }

            student.DateDeleted = DateTime.UtcNow;

            try
            {
                context.Update(student);
                await context.SaveChangesAsync();
            }
            catch
            {
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
        [UseDbContext(typeof(AppDbContext)), UseProjection,
         HotChocolate.AspNetCore.Authorization.Authorize(Policy = Privilege.PRIVILEGE_MANAGE_STUDENTS)]
        public async Task<IQueryable<Student>> RestoreStudentAsync(
            long id,
            [ScopedService] AppDbContext context
        )
        {
            Student? student = await context.Students
                    .Where(x => x.Id == id && x.DateDeleted != null)
                    .SingleOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            student.DateDeleted = null;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return context.Students.Where(x => x.Id == id);
        }
    }
}
