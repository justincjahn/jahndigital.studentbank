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
using JahnDigital.StudentBank.WebApi.Extensions;
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
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting, Authorize]
        public async Task<IQueryable<StudentStock>> GetStudentStocksAsync(
            long studentId,
            [ScopedService] AppDbContext context,
            [Service] IResolverContext resolverContext
        )
        {
            await resolverContext
                .SetDataOwner()
                .AssertAuthorizedAsync($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageStocks}>");

            return context
                .StudentStocks
                .Where(x => x.StudentId == studentId);
        }

        /// <summary>
        ///     Get the purchase history for a student's stock.
        /// </summary>
        /// <param name="studentStockId"></param>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseProjection, UseFiltering, UseSorting, Authorize]
        public async Task<IQueryable<StudentStockHistory>> GetStudentStockHistoryAsync(
            long studentStockId,
            [ScopedService] AppDbContext context,
            [Service] IResolverContext resolverContext
        )
        {
            StudentStock studentStock = await context
                    .StudentStocks
                    .Where(x => x.Id == studentStockId)
                    .FirstOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            await resolverContext
                .SetDataOwner(studentStock.StudentId, resolverContext.GetUserType())
                .AssertAuthorizedAsync($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageStocks}>");

            return context
                .StudentStockHistory
                .Where(x => x.StudentStockId == studentStockId);
        }
    }
}
