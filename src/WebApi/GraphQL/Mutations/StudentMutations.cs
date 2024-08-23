using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Application.Common.DTOs;
using JahnDigital.StudentBank.Application.Students.Commands.AdminUpdateStudent;
using JahnDigital.StudentBank.Application.Students.Commands.AuthenticateStudent;
using JahnDigital.StudentBank.Application.Students.Commands.AuthenticateStudentInvite;
using JahnDigital.StudentBank.Application.Students.Commands.DeleteStudent;
using JahnDigital.StudentBank.Application.Students.Commands.NewStudent;
using JahnDigital.StudentBank.Application.Students.Commands.RefreshStudentToken;
using JahnDigital.StudentBank.Application.Students.Commands.RegisterStudent;
using JahnDigital.StudentBank.Application.Students.Commands.RestoreStudent;
using JahnDigital.StudentBank.Application.Students.Commands.RevokeStudentToken;
using JahnDigital.StudentBank.Application.Students.Commands.UpdateStudent;
using JahnDigital.StudentBank.Application.Students.Queries.GetStudent;
using JahnDigital.StudentBank.Application.Students.Queries.GetStudents;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.WebApi.Extensions;
using JahnDigital.StudentBank.WebApi.Models;
using MediatR;
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
        /// <param name="mediatr"></param>
        /// <param name="contextAccessor"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<AuthenticateResponse> StudentLoginAsync(
            AuthenticateRequest input,
            [Service] ISender mediatr,
            [Service] IHttpContextAccessor contextAccessor,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrEmpty(input.Username) || string.IsNullOrEmpty(input.Password))
            {
                throw ErrorFactory.Unauthorized();
            }

            var command = new AuthenticateStudentCommand(input.Username, input.Password, GetIp(contextAccessor));

            try
            {
                var response = await mediatr.Send(command, cancellationToken);
                SetTokenCookie(contextAccessor, response.RefreshToken);
                return response;
            }
            catch (Exception)
            {
                throw ErrorFactory.LoginFailed();
            }
        }

        /// <summary>
        ///     Obtain a new JWT token using a refresh token.
        /// </summary>
        /// <param name="token">The refresh token to use when obtaining a new JWT token. Must be valid and not expired.</param>
        /// <param name="mediatr"></param>
        /// <param name="contextAccessor"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<AuthenticateResponse> StudentRefreshTokenAsync(
            string? token,
            [Service] ISender mediatr,
            [Service] IHttpContextAccessor contextAccessor,
            CancellationToken cancellationToken
        )
        {
            token = token ?? GetToken(contextAccessor) ?? throw ErrorFactory.InvalidRefreshToken();

            var command = new RefreshStudentTokenCommand(token, GetIp(contextAccessor));

            try
            {
                var response = await mediatr.Send(command, cancellationToken);
                SetTokenCookie(contextAccessor, response.RefreshToken);
                return response;
            }
            catch (Exception)
            {
                throw ErrorFactory.InvalidRefreshToken();
            }
        }

        /// <summary>
        ///     Revoke a refresh token.
        /// </summary>
        /// <param name="token">The refresh token to revoke.</param>
        /// <param name="mediatr"></param>
        /// <param name="contextAccessor"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<bool> StudentRevokeRefreshTokenAsync(
            string? token,
            [Service] ISender mediatr,
            [Service] IHttpContextAccessor contextAccessor,
            CancellationToken cancellationToken
        )
        {
            token ??= GetToken(contextAccessor) ?? throw ErrorFactory.InvalidRefreshToken();

            var command = new RevokeStudentTokenCommand(token, GetIp(contextAccessor));

            try
            {
                await mediatr.Send(command, cancellationToken);
                ClearTokenCookie(contextAccessor);
                return true;
            }
            catch (Exception)
            {
                throw ErrorFactory.NotFound(nameof(token), token);
            }
        }

        /// <summary>
        ///     Attempt to generate a preauthorization token from the provided input.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A temporary JWT token if preauthorization is successful.</returns>
        public async Task<string> StudentPreregistrationAsync(
            StudentPreauthenticationRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var command = new AuthenticateStudentInviteCommand(input.InviteCode, input.AccountNumber);
                var response = await mediatr.Send(command, cancellationToken);
                return response;
            }
            catch (Exception)
            {
                throw ErrorFactory.InvalidInviteCode(input.InviteCode, input.AccountNumber);
            }
        }

        /// <summary>
        ///     Register a student using the preauthorization token and provided input.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="httpContext"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>True if registration is successful, otherwise an error message.</returns>
        [Authorize(Policy = Constants.AuthPolicy.Preauthorization)]
        public async Task<bool> StudentRegistrationAsync(
            StudentRegisterRequest input,
            [Service] IHttpContextAccessor httpContext,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            if (httpContext.HttpContext is null)
            {
                throw ErrorFactory.QueryFailed("Unable to obtain HttpContext.");
            }

            Claim userIdClaim = httpContext.HttpContext.User.Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)
            ?? throw ErrorFactory.Unauthorized();

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                throw ErrorFactory.Unauthorized();
            }

            Claim userTypeClaim = httpContext.HttpContext.User.Claims
                .FirstOrDefault(x => x.Type == Constants.Auth.CLAIM_USER_TYPE)
            ?? throw ErrorFactory.Unauthorized();

            if (userTypeClaim.Value != UserType.Student.Name)
            {
                throw ErrorFactory.Unauthorized();
            }

            var command = new RegisterStudentCommand(userId, DateTime.UtcNow, input.Password, input.Email);
            await mediatr.Send(command, cancellationToken);

            return true;
        }

        /// <summary>
        ///     Update an existing student.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="resolverContext"></param>
        /// <param name="mediatr"></param>
        /// <param name="contextAccessor"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize]
        public async Task<IQueryable<Student>> UpdateStudentAsync(
            UpdateStudentRequest input,
            [SchemaService] IResolverContext resolverContext,
            [Service] ISender mediatr,
            [Service] IHttpContextAccessor contextAccessor,
            CancellationToken cancellationToken
        )
        {
            await resolverContext
                .SetDataOwner(input.Id, UserType.Student)
                .AssertAuthorizedAsync($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageStudents}>");

            Student student = await (await mediatr.Send(new GetStudentQuery(input.Id), cancellationToken))
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw ErrorFactory.NotFound(nameof(Student), input.Id);

            if (input.GroupId is not null && resolverContext.GetUserType() != UserType.User)
            {
                throw ErrorFactory.Unauthorized();
            }

            if (input.Password is not null && resolverContext.GetUserType() != UserType.User)
            {
                if (input.CurrentPassword is null)
                {
                    throw ErrorFactory.LoginFailed();
                }

                var authCommand = new AuthenticateStudentCommand(
                    student.AccountNumber,
                    input.CurrentPassword,
                    GetIp(contextAccessor)
                );

                try
                {
                    await mediatr.Send(authCommand, cancellationToken);
                }
                catch
                {
                    throw ErrorFactory.LoginFailed();
                }
            }

            var command = new UpdateStudentCommand
            {
                Id = student.Id,
                AccountNumber = input.AccountNumber?.PadLeft(10, '0'),
                FirstName = input.FirstName,
                LastName = input.LastName,
                Email = input.Email,
                Password = input.Password,
            };

            await mediatr.Send(command, cancellationToken);

            return await mediatr.Send(new GetStudentQuery(input.Id), cancellationToken);
        }

        /// <summary>
        ///     Update a group of students at once.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_STUDENTS)]
        public async Task<IQueryable<Student>> UpdateBulkStudentAsync(
            IEnumerable<UpdateStudentRequest> input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            IEnumerable<UpdateStudentRequest> updateStudentRequests = input as UpdateStudentRequest[] ?? input.ToArray();
            IEnumerable<long> ids = updateStudentRequests.Select(x => x.Id).ToArray();

            if (!ids.Any())
            {
                throw ErrorFactory.NotFound(nameof(Student));
            }

            var students = await (await mediatr.Send(new GetStudentsQuery(), cancellationToken))
                .Where(x => ids.Contains(x.Id))
                .ToListAsync(cancellationToken);

            if (students.Count != ids.Count())
            {
                throw ErrorFactory.NotFound(nameof(Student));
            }

            foreach (var request in updateStudentRequests)
            {
                var student = students.Find(x => x.Id == request.Id)
                    ?? throw ErrorFactory.NotFound(nameof(Student), request.Id);

                var command = new AdminUpdateStudentCommand(
                    student.Id,
                    request.AccountNumber,
                    request.Email,
                    request.FirstName,
                    request.LastName,
                    request.GroupId,
                    request.Password
                );

                await mediatr.Send(command, cancellationToken);
            }

            return (await mediatr.Send(new GetStudentsQuery(), cancellationToken))
                .Where(x => ids.Contains(x.Id));
        }

        /// <summary>
        ///     Create a new student.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_STUDENTS)]
        public async Task<IQueryable<Student>> NewStudentAsync(
            NewStudentRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            var request = new NewStudentCommand
            {
                AccountNumber = input.AccountNumber,
                GroupId = input.GroupId,
                FirstName = input.FirstName,
                LastName = input.LastName,
                Password = input.Password,
                Email = input.Email
            };

            var studentId = await mediatr.Send(request, cancellationToken);
            return await mediatr.Send(new GetStudentQuery(studentId), cancellationToken);
        }

        /// <summary>
        ///     Delete a student.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Policy = Privilege.PRIVILEGE_MANAGE_STUDENTS)]
        public async Task<bool> DeleteStudentAsync(
            long id,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new DeleteStudentCommand(id), cancellationToken);
            return true;
        }

        /// <summary>
        ///     Restore a deleted student.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_STUDENTS)]
        public async Task<IQueryable<Student>> RestoreStudentAsync(
            long id,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new RestoreStudentCommand(id), cancellationToken);
            return await mediatr.Send(new GetStudentQuery(id), cancellationToken);
        }
    }
}
