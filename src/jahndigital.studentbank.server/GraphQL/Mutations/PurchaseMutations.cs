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

namespace jahndigital.studentbank.server.GraphQL.Mutations
{
    /// <summary>
    ///     CRUD operations for <see cref="StudentPurchase" /> entities.
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
        [UseDbContext(typeof(AppDbContext)), HotChocolate.AspNetCore.Authorization.Authorize]
        public async Task<IQueryable<StudentPurchase>> NewPurchaseAsync(
            PurchaseRequest input,
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

            StudentPurchase? purchase;
            purchase = await transactionService.PurchaseAsync(input);

            return context.StudentPurchases.Where(x => x.Id == purchase.Id);
        }
    }
}
