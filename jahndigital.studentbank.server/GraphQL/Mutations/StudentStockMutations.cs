using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.services.DTOs;
using jahndigital.studentbank.services.Interfaces;
using jahndigital.studentbank.utils;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.server.GraphQL.Mutations
{
    /// <summary>
    ///     CRUD operations for stock purchases.
    /// </summary>
    [ExtendObjectType(Name = "Mutation")]
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
        [UseSelection, Authorize]
        public async Task<IQueryable<StudentStock>> NewStockPurchaseAsync(
            PurchaseStockRequest input,
            [Service] AppDbContext context,
            [Service] ITransactionService transactionService,
            [Service] IResolverContext resolverContext
        )
        {
            // Fetch the student ID that owns the share and validate they are authorized
            var studentId = await context.Shares
                    .Where(x => x.Id == input.ShareId)
                    .Select(x => (long?) x.StudentId)
                    .FirstOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            resolverContext.SetUser(studentId, Constants.UserType.Student);
            var auth = await resolverContext.AuthorizeAsync(
                $"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManageStudents}>"
            );

            if (!auth.Succeeded) {
                throw ErrorFactory.Unauthorized();
            }

            StudentStock? purchase;
            purchase = await transactionService.PurchaseStockAsync(input);

            return context.StudentStocks.Where(x => x.Id == purchase.Id);
        }
    }
}