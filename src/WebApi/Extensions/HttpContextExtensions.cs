using System.Linq;
using System.Security.Claims;
using JahnDigital.StudentBank.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Constants = JahnDigital.StudentBank.Application.Common.Constants;

namespace JahnDigital.StudentBank.WebApi.Extensions;

/// <summary>
/// Extensions that enable quick access to specific JWT claims.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Get the <see cref="UserType"/> value representing the logged in user or student, or <see cref="UserType.Anonymous"/>.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static UserType GetUserType(this HttpContext context) => context.User.GetUserType();

    /// <summary>
    /// Get the ID number of the logged in user or student, or -1.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static long GetUserId(this HttpContext context) => context.User.GetUserId();

    /// <summary>
    /// Get the user's current role.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static Role GetUserRole(this HttpContext context) => context.User.GetUserRole();
}
