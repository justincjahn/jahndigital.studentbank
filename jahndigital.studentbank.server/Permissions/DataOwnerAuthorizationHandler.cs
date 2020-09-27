using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace jahndigital.studentbank.server.Permissions
{
    /// <summary>
    /// Validates that the user id contained in the request URL matches the authenticated user.
    /// </summary>
    internal class DataOwnerAuthorizationHandler : AuthorizationHandler<DataOwnerRequirement>
    {
        private readonly IHttpContextAccessor _httpContext;

        public DataOwnerAuthorizationHandler(IHttpContextAccessor context) => _httpContext = context;

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DataOwnerRequirement requirement)
        {
            bool isStudent = false;
            string userId = String.Empty;

            if (_httpContext.HttpContext.Request.RouteValues.ContainsKey("userId")) {
                userId = _httpContext.HttpContext.Request.RouteValues["userId"].ToString();
            }

            if (_httpContext.HttpContext.Request.RouteValues.ContainsKey("studentId")) {
                userId = _httpContext.HttpContext.Request.RouteValues["studentId"].ToString();
                isStudent = true;
            }

            if (string.IsNullOrWhiteSpace(userId)) {
                return Task.CompletedTask;
            }

            var claim = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (claim == null) return Task.CompletedTask;

            var userType = context.User.Claims.FirstOrDefault(x => x.Type == Constants.Auth.CLAIM_USER_TYPE);
            if  (userType == null) return Task.CompletedTask;

            // If the user is accessing the wrong type of resource.
            if (isStudent && userType.Value != Constants.UserType.Student.Name) return Task.CompletedTask;

            int routeId, claimId;
            if (!int.TryParse(userId, out routeId)) return Task.CompletedTask;
            if (!int.TryParse(claim.Value, out claimId)) return Task.CompletedTask;

            if (routeId == claimId) {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
