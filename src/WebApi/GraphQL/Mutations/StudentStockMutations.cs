using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Application.Transactions.DTOs;
using JahnDigital.StudentBank.Application.Transactions.Services;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Mutations
{
    /// <summary>
    ///     CRUD operations for stock purchases.
    /// </summary>
    [ExtendObjectType("Mutation")]
    public class StudentStockMutations
    {
        /// <summary>
        ///     Attempt to buy or sell the provided stock.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <param name="transactionService"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UseProjection, HotChocolate.AspNetCore.Authorization.Authorize]
        public async Task<IQueryable<StudentStock>> NewStockPurchaseAsync(
            PurchaseStockRequest input,
            [ScopedService] AppDbContext context,
            [Service] ITransactionService transactionService,
            [Service] IResolverContext resolverContext
        )
        {
            // Fetch the student ID that owns the share and validate they are authorized
            long studentId = await context.Shares
                    .Where(x => x.Id == input.ShareId)
                    .Select(x => (long?)x.StudentId)
                    .FirstOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            resolverContext.SetUser(studentId, UserType.Student);
            AuthorizationResult? auth = await resolverContext.AuthorizeAsync(
                $"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageStudents}>"
            );

            if (!auth.Succeeded)
            {
                throw ErrorFactory.Unauthorized();
            }

            StudentStock? purchase;
            purchase = await transactionService.PurchaseStockAsync(input);

            return context.StudentStocks.Where(x => x.Id == purchase.Id);
        }
    }
}
