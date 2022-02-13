using System.Linq;
using System.Security.Claims;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Domain.Enums;

namespace JahnDigital.StudentBank.WebApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static long GetUserId(this ClaimsPrincipal claim)
    {
        var userId = claim
            .Claims
            .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)
            ?.Value
        ?? "-1";

        return long.Parse(userId);
    }

    public static UserType GetUserType(this ClaimsPrincipal claim)
    {
        var type = claim
            .Claims
            .FirstOrDefault(x => x.Type == Constants.Auth.CLAIM_USER_TYPE)
            ?.Value
        ?? UserType.Anonymous.Name;

        return UserType
            .UserTypes
            .FirstOrDefault(x => x.Name == type)
        ?? UserType.Anonymous;
    }

    public static Role GetUserRole(this ClaimsPrincipal claim)
    {
        var role = claim
            .Claims
            .FirstOrDefault(x => x.Type == ClaimTypes.Role)
            ?.Value
        ?? Role.ROLE_STUDENT;

        return Role
            .Roles
            .FirstOrDefault(x => x.Name == role)
        ?? Role.Student;
    }
}
