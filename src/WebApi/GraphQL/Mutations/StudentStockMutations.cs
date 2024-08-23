using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Application.Shares.Queries.GetShare;
using JahnDigital.StudentBank.Application.Stocks.Queries.GetStudentStock;
using JahnDigital.StudentBank.Application.Transactions.DTOs;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.WebApi.Extensions;
using MediatR;
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
        /// <param name="mediatr"></param>
        /// <param name="resolverContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseProjection, Authorize]
        public async Task<IQueryable<StudentStock>> NewStockPurchaseAsync(
            PurchaseStockRequest input,
            [SchemaService] IResolverContext resolverContext,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            // Fetch the student ID that owns the share and validate they are authorized
            var studentId = await (await mediatr.Send(new GetShareQuery(input.ShareId), cancellationToken))
                .Select(x => (long?)x.StudentId)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw ErrorFactory.NotFound(nameof(Share), input.ShareId);

            await resolverContext
                .SetDataOwner(studentId, UserType.Student)
                .AssertAuthorizedAsync($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageStudents}>");

            var studentStockId = await mediatr.Send(input, cancellationToken);
            return await mediatr.Send(new GetStudentStockQuery(studentStockId), cancellationToken);
        }
    }
}
