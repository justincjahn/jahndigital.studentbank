using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.server.Models;
using jahndigital.studentbank.server.Services;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.server.GraphQL.Mutations
{
    /// <summary>
    /// CRUD operations for stock purchases.
    /// </summary>
    [ExtendObjectType(Name = "Mutation")]
    public class StudentStockMutations
    {
        /// <summary>
        /// Attempt to buy or sell the provided stock.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <param name="transactionService"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseSelection, Authorize]
        public async Task<IQueryable<dal.Entities.StudentStock>> NewStockPurchaseAsync(
            PurchaseStockRequest input,
            [Service] AppDbContext context,
            [Service] ITransactionService transactionService,
            [Service] IResolverContext resolverContext
        ) {
            // Fetch the student ID that owns the share and validate they are authorized
            long studentId = await context.Shares
                .Where(x => x.Id == input.ShareId)
                .Select(x => (long?)x.StudentId)
                .FirstOrDefaultAsync()
            ?? throw ErrorFactory.NotFound();

            resolverContext.SetUser(studentId, Constants.UserType.Student);
            var auth = await resolverContext.AuthorizeAsync(
                $"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManageStudents}>"
            );

            if (!auth.Succeeded) throw ErrorFactory.Unauthorized();

            dal.Entities.StudentStock? purchase;
            try {
                purchase = await transactionService.PurchaseStockAsync(input);
            } catch (Exception e) {
                throw e;
            }

            return context.StudentStocks.Where(x => x.Id == purchase.Id);
        }
    }
}
