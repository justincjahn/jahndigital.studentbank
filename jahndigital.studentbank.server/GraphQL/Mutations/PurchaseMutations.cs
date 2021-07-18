using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
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
    ///     CRUD operations for <see cref="dal.Entities.StudentPurchase" /> entities.
    /// </summary>
    [ExtendObjectType("Mutation")]
    public class PurchaseMutations
    {
        /// <summary>
        ///     Make a new purchase, if authorized.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <param name="transactionService"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), Authorize]
        public async Task<IQueryable<StudentPurchase>> NewPurchaseAsync(
            PurchaseRequest input,
            [ScopedService] AppDbContext context,
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

            StudentPurchase? purchase;
            purchase = await transactionService.PurchaseAsync(input);

            return context.StudentPurchases.Where(x => x.Id == purchase.Id);
        }
    }
}
