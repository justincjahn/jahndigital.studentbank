using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Application.Shares.Queries.GetShare;
using JahnDigital.StudentBank.Application.Transactions.Queries.GetShareTransactions;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.WebApi.Extensions;
using JahnDigital.StudentBank.WebApi.GraphQL.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Queries
{
    /// <summary>
    /// </summary>
    [ExtendObjectType("Query")]
    public class TransactionQueries : RequestBase
    {
        /// <summary>
        ///     Get transactions by Student ID and Share ID
        /// </summary>
        /// <param name="shareId"></param>
        /// <param name="resolverContext"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UsePaging, UseFiltering, UseSorting, Authorize]
        public async Task<IQueryable<Transaction>> GetTransactionsAsync(
            long shareId,
            [Service] IResolverContext resolverContext,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            var share = await (await mediatr.Send(new GetShareQuery(shareId), cancellationToken))
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw ErrorFactory.NotFound();

            await resolverContext
                .SetDataOwner(share.StudentId, UserType.Student)
                .AssertAuthorizedAsync($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageTransactions}>");

            return await mediatr.Send(new GetShareTransactionsQuery(shareId), cancellationToken);
        }
    }
}
