using System.Linq;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using jahndigital.studentbank.dal.Contexts;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    [ExtendObjectType(Name = "Query")]
    public class StudentStockQueries
    {
        /// <summary>
        /// Get the stocks purchased by the user.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UsePaging, UseSelection, UseSorting, UseFiltering, Authorize]
        public IQueryable<dal.Entities.StudentStock> GetStudentStocks(
            [Service] AppDbContext context,
            [Service] IResolverContext resolverContext
        ) {
            var id = resolverContext.GetUserId() ?? throw ErrorFactory.NotFound();
            var type = resolverContext.GetUserType() ?? throw ErrorFactory.NotFound();
            resolverContext.SetUser(id, type);
            if (type == Constants.UserType.User) return context.StudentStocks;
            return context.StudentStocks.Where(x => x.StudentId == id);
        }
    }
}
