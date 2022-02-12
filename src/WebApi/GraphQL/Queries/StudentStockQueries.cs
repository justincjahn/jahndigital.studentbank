using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Queries
{
    [ExtendObjectType("Query")]
    public class StudentStockQueries
    {
        /// <summary>
        ///     Get the stocks purchased by the student specified.
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         HotChocolate.AspNetCore.Authorization.Authorize]
        public async Task<IQueryable<StudentStock>> GetStudentStocksAsync(
            long studentId,
            [ScopedService] AppDbContext context,
            [Service] IResolverContext resolverContext
        )
        {
            long id = resolverContext.GetUserId() ?? throw ErrorFactory.NotFound();
            UserType? type = resolverContext.GetUserType() ?? throw ErrorFactory.NotFound();
            resolverContext.SetUser(studentId, type);

            AuthorizationResult? auth = await resolverContext
                .AuthorizeAsync($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageStocks}>");

            if (!auth.Succeeded)
            {
                throw ErrorFactory.Unauthorized();
            }

            return context.StudentStocks.Where(x => x.StudentId == studentId);
        }

        /// <summary>
        ///     Get the purchase history for a student's stock.
        /// </summary>
        /// <param name="studentStockId"></param>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting,
         HotChocolate.AspNetCore.Authorization.Authorize]
        public async Task<IQueryable<StudentStockHistory>> GetStudentStockHistoryAsync(
            long studentStockId,
            [ScopedService] AppDbContext context,
            [Service] IResolverContext resolverContext
        )
        {
            UserType? type = resolverContext.GetUserType() ?? throw ErrorFactory.NotFound();

            StudentStock? studentStock = await context.StudentStocks
                    .Where(x => x.Id == studentStockId)
                    .FirstOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            resolverContext.SetUser(studentStock.StudentId, type);

            AuthorizationResult? auth = await resolverContext
                .AuthorizeAsync($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageStocks}>");

            if (!auth.Succeeded)
            {
                throw ErrorFactory.Unauthorized();
            }

            return context.StudentStockHistory.Where(x => x.StudentStockId == studentStockId);
        }
    }
}
