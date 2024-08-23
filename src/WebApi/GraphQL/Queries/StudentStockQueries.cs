using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Application.Stocks.Queries.GetStudentStock;
using JahnDigital.StudentBank.Application.Stocks.Queries.GetStudentStockHistory;
using JahnDigital.StudentBank.Application.Stocks.Queries.GetStudentStocks;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.WebApi.Extensions;
using JahnDigital.StudentBank.WebApi.GraphQL.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Queries
{
    [ExtendObjectType("Query")]
    public class StudentStockQueries : RequestBase
    {
        /// <summary>
        ///     Get the stocks purchased by the student specified.
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="resolverContext"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UsePaging, UseProjection, UseFiltering, UseSorting, Authorize]
        public async Task<IQueryable<StudentStock>> GetStudentStocksAsync(
            long studentId,
            [SchemaService] IResolverContext resolverContext,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await resolverContext
                .SetDataOwner()
                .AssertAuthorizedAsync($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageStocks}>");

            return await mediatr.Send(new GetStudentStocksQuery(studentId), cancellationToken);
        }

        /// <summary>
        ///     Get the purchase history for a student's stock.
        /// </summary>
        /// <param name="studentStockId"></param>
        /// <param name="resolverContext"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UsePaging, UseProjection, UseFiltering, UseSorting, Authorize]
        public async Task<IQueryable<StudentStockHistory>> GetStudentStockHistoryAsync(
            long studentStockId,
            [SchemaService] IResolverContext resolverContext,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            var studentStock = await (await mediatr.Send(new GetStudentStockQuery(studentStockId), cancellationToken))
                .FirstOrDefaultAsync(cancellationToken: cancellationToken)
            ?? throw ErrorFactory.NotFound();

            await resolverContext
                .SetDataOwner(studentStock.StudentId, resolverContext.GetUserType())
                .AssertAuthorizedAsync($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageStocks}>");

            return await mediatr.Send(new GetStudentStockHistoryQuery(studentStockId), cancellationToken);
        }
    }
}
