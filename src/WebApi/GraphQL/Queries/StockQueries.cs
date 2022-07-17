using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Stocks.Queries.GetStockHistory;
using JahnDigital.StudentBank.Application.Stocks.Queries.GetStocks;
using JahnDigital.StudentBank.Application.Stocks.Queries.GetStocksForStudent;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.WebApi.Extensions;
using JahnDigital.StudentBank.WebApi.GraphQL.Common;
using MediatR;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Queries
{
    [ExtendObjectType("Query")]
    public class StockQueries : RequestBase
    {
        /// <summary>
        ///     Get a list of stocks available to the user.
        /// </summary>
        /// <param name="instances">One or more instances to use when filtering.</param>
        /// <param name="resolverContext"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UsePaging, UseProjection, UseFiltering, UseSorting, Authorize]
        public async Task<IQueryable<Stock>> GetStocksAsync(
            IEnumerable<long>? instances,
            [SchemaService] IResolverContext resolverContext,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            resolverContext.SetDataOwner();

            if (resolverContext.GetUserType() == UserType.User)
            {
                await resolverContext.AssertAuthorizedAsync(Privilege.ManageStocks.Name);
                return await mediatr.Send(new GetStocksQuery(instances), cancellationToken);
            }

            return await mediatr.Send(new GetStocksForStudentQuery(resolverContext.GetUserId()), cancellationToken);
        }

        /// <summary>
        ///     Get a list of history for a given stock.
        /// </summary>
        /// <param name="stockId"></param>
        /// <param name="resolverContext"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UsePaging, UseProjection, UseFiltering, UseSorting, Authorize]
        public async Task<IQueryable<StockHistory>> GetStockHistoryAsync(
            long stockId,
            [SchemaService] IResolverContext resolverContext,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            resolverContext.SetDataOwner();

            if (resolverContext.GetUserType() == UserType.User)
            {
                await resolverContext.AssertAuthorizedAsync(Privilege.ManageStocks.Name);
                return await mediatr.Send(new GetStockHistoryQuery(stockId), cancellationToken);
            }

            var availableStocks = await mediatr
                .Send(new GetStocksForStudentQuery(resolverContext.GetUserId()), cancellationToken);

            if (!availableStocks.Any(x => x.Id == stockId)) throw ErrorFactory.NotFound();

            return await mediatr.Send(new GetStockHistoryQuery(stockId), cancellationToken);
        }

        /// <summary>
        ///     Get all deleted stocks.
        /// </summary>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UsePaging, UseProjection, UseFiltering, UseSorting, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public async Task<IQueryable<Stock>> GetDeletedStocksAsync(
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            return await mediatr.Send(new GetStocksQuery(null, true), cancellationToken);
        }
    }
}
