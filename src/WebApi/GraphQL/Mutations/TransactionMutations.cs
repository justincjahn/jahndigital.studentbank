using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Application.Shares.Queries.GetShare;
using JahnDigital.StudentBank.Application.Transactions.Commands.PostBulkTransaction;
using JahnDigital.StudentBank.Application.Transactions.DTOs;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.WebApi.Extensions;
using JahnDigital.StudentBank.WebApi.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Mutations
{
    /// <summary>
    ///     CRUD operations for <see cref="Transaction" /> entities.
    /// </summary>
    [ExtendObjectType("Mutation")]
    public class TransactionMutations
    {
        /// <summary>
        ///     Post a <see cref="Transaction" /> to the provided share, if authorized.
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = Privilege.PRIVILEGE_MANAGE_TRANSACTIONS)]
        public async Task<Transaction> NewTransactionAsync(
            TransactionRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            return await mediatr.Send(input, cancellationToken);
        }

        /// <summary>
        ///     Perform a bulk transaction and return transaction information.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="skipBelowNegative">
        ///     Continue posting other transactions if a withdrawal cannot be completed due to nonsufficient funds
        ///     while the <c>takeNegative</c> flag is set to <see langword="false" />.
        /// </param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Policy = Privilege.PRIVILEGE_MANAGE_TRANSACTIONS)]
        public async Task<IQueryable<Transaction>> NewBulkTransactionAsync(
            IEnumerable<NewTransactionRequest> input,
            [Service] ISender mediatr,
            bool skipBelowNegative = false,
            CancellationToken cancellationToken = new()
        )
        {
            var transactions = input.Select(x => new TransactionRequest()
            {
                Amount = x.Amount,
                Comment = x.Comment,
                ShareId = x.ShareId,
                TakeNegative = x.TakeNegative
            });

            return await mediatr.Send(new PostBulkTransactionCommand(transactions, !skipBelowNegative, false), cancellationToken);
        }

        /// <summary>
        ///     Transfer funds from one share to another, if authorized.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="resolverContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<Tuple<Transaction, Transaction>> NewTransferAsync(
            NewTransferRequest input,
            [Service] ISender mediatr,
            [SchemaService] IResolverContext resolverContext,
            CancellationToken cancellationToken
        )
        {
            // Fetch the student ID that owns the share and validate they are authorized
            var studentId = await (await mediatr.Send(new GetShareQuery(input.SourceShareId), cancellationToken))
                .Select(x => (long?)x.StudentId)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw ErrorFactory.NotFound(nameof(Share), input.SourceShareId);

            await resolverContext
                .SetDataOwner(studentId, UserType.Student)
                .AssertAuthorizedAsync($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageStudents}>");

            var transactions = await mediatr.Send(
                new TransferRequest()
                {
                    Amount = input.Amount,
                    Comment = input.Comment,
                    WithdrawalLimit = resolverContext.GetUserType() != UserType.User,
                    DestinationShareId = input.DestinationShareId,
                    SourceShareId = input.SourceShareId
                },

                cancellationToken
            );

            return transactions.ToTuple();
        }

        /// <summary>
        ///     Post dividends for a specific <see cref="ShareType" /> and a group of <see cref="Instance" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Policy = Privilege.PRIVILEGE_MANAGE_TRANSACTIONS)]
        public async Task<bool> PostDividendsAsync(
            PostDividendsRequest input,
            [Service] ISender mediatr,
            CancellationToken cancellationToken
        )
        {
            await mediatr.Send(input, cancellationToken);
            return true;
        }
    }
}
