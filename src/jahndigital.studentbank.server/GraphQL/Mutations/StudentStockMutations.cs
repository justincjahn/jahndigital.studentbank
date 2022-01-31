using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.services.DTOs;
using jahndigital.studentbank.services.Interfaces;
using jahndigital.studentbank.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.server.GraphQL.Mutations
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

            resolverContext.SetUser(studentId, Constants.UserType.Student);
            AuthorizationResult? auth = await resolverContext.AuthorizeAsync(
                $"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManageStudents}>"
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
