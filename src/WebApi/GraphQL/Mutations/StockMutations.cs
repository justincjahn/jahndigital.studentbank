using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Stocks.Commands.DeleteStock;
using JahnDigital.StudentBank.Application.Stocks.Commands.LinkStock;
using JahnDigital.StudentBank.Application.Stocks.Commands.NewStock;
using JahnDigital.StudentBank.Application.Stocks.Commands.PurgeStockHistory;
using JahnDigital.StudentBank.Application.Stocks.Commands.RestoreStock;
using JahnDigital.StudentBank.Application.Stocks.Commands.UnlinkStock;
using JahnDigital.StudentBank.Application.Stocks.Commands.UpdateStock;
using JahnDigital.StudentBank.Application.Stocks.Queries.GetStock;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using JahnDigital.StudentBank.WebApi.Models;
using MediatR;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Mutations
{
    /// <summary>
    ///     CRUD operations for <see cref="Stock" /> entities.
    /// </summary>
    [ExtendObjectType("Mutation")]
    public class StockMutations
    {
        /// <summary>
        ///     Create a new stock.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Policy = Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public async Task<IQueryable<Stock>> NewStockAsync(
            NewStockRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            var stockId = await mediatr.Send(new NewStockCommand(input.Symbol, input.Name, input.TotalShares, input.CurrentValue), cancellationToken);
            return await mediatr.Send(new GetStockQuery(stockId), cancellationToken);
        }

        /// <summary>
        ///     Update a <see cref="Stock" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Policy = Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public async Task<IQueryable<Stock>> UpdateStockAsync(
            UpdateStockRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(
                new UpdateStockCommand
                {
                    StockId = input.Id,
                    Name = input.Name,
                    Symbol = input.Symbol,
                    RawDescription = input.RawDescription,
                    CurrentValue = input.CurrentValue
                }, cancellationToken);

            return await mediatr.Send(new GetStockQuery(input.Id), cancellationToken);
        }

        /// <summary>
        /// Purge stock history for a given stock to a given date.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Policy = Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public async Task<IEnumerable<StockHistory>> PurgeStockHistoryAsync(
            PurgeStockRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            return await mediatr.Send(new PurgeStockHistoryCommand(input.StockId, input.Date), cancellationToken);
        }

        /// <summary>
        ///     Link a <see cref="Stock" /> to an <see cref="Instance" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Policy = Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public async Task<IQueryable<Stock>> LinkStockAsync(
            LinkStockRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new LinkStockCommand(input.StockId, input.InstanceId), cancellationToken);
            return await mediatr.Send(new GetStockQuery(input.StockId), cancellationToken);
        }

        /// <summary>
        ///     Unlink a <see cref="Stock" /> from an <see cref="Instance" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Policy = Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public async Task<IQueryable<Stock>> UnlinkStockAsync(
            LinkStockRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new UnlinkStockCommand(input.StockId, input.InstanceId), cancellationToken);
            return await mediatr.Send(new GetStockQuery(input.StockId), cancellationToken);
        }

        /// <summary>
        ///     Soft-delete a stock.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Policy = Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public async Task<bool> DeleteStockAsync(
            long id,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new DeleteStockCommand(id), cancellationToken);
            return true;
        }

        /// <summary>
        ///     Restore a deleted stock.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)),
         UseProjection, Authorize(Policy = Privilege.PRIVILEGE_MANAGE_STOCKS)]
        public async Task<IQueryable<Stock>> RestoreStockAsync(
            long id,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(new RestoreStockCommand(id), cancellationToken);
            return await mediatr.Send(new GetStockQuery(id), cancellationToken);
        }
    }
}
