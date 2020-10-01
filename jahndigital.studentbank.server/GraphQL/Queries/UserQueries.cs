using System.Linq;
using System.Security.Claims;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;
using Microsoft.AspNetCore.Http;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    /// <summary>
    /// 
    /// </summary>
    [ExtendObjectType(Name = "Query")]
    public class UserQueries
    {
        /// <summary>
        /// Get the currently logged in user information (if the user is a user).
        /// </summary>
        /// <param name="contextAccessor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UseSelection]
        public IQueryable<dal.Entities.User> GetCurrentUser([Service] IHttpContextAccessor contextAccessor, [Service] AppDbContext context)
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

            if (type.Value != Constants.UserType.User.Name) {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("User not found or not logged in.")
                        .SetCode("NOT_FOUND")
                        .Build()
                );
            }

            return context.Users.Where(x => x.Id == int.Parse(id.Value));
        }

        /// <summary>
        /// Returns true if the user is authenticated.
        /// </summary>
        /// <param name="contextAccessor"></param>
        /// <returns></returns>
        public bool IsAuthenticated([Service] IHttpContextAccessor contextAccessor)
            => contextAccessor.HttpContext.User.Identity.IsAuthenticated;
    }
}
