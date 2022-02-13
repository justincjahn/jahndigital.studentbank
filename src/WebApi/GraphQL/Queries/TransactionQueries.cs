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
    /// <summary>
    /// </summary>
    [ExtendObjectType("Query")]
    public class TransactionQueries
    {
        /// <summary>
        ///     Get transactions by Student ID and Share ID
        /// </summary>
        /// <param name="shareId"></param>
        /// <param name="context"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), UsePaging, UseFiltering, UseSorting, Authorize]
        public async Task<IQueryable<Transaction>> GetTransactionsAsync(
            long shareId,
            [ScopedService] AppDbContext context,
            [Service] IResolverContext resolverContext
        )
        {
            Share share = await context
                    .Shares
                    .Where(x => x.Id == shareId)
                    .FirstOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            await resolverContext
                .SetDataOwner(share.StudentId, UserType.Student)
                .AssertAuthorizedAsync($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageTransactions}>");

            return context.Transactions.Where(x => x.TargetShareId == shareId);
        }
    }
}
