using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.server.Models;
using jahndigital.studentbank.services.DTOs;
using jahndigital.studentbank.services.Interfaces;
using jahndigital.studentbank.utils;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.server.GraphQL.Mutations
{
    /// <summary>
    ///     CRUD operations for <see cref="dal.Entities.Transaction" /> entities.
    /// </summary>
    [ExtendObjectType("Mutation")]
    public class TransactionMutations
    {
        /// <summary>
        ///     Post a <see cref="dal.Entities.Transaction" /> to the provided share, if authorized.
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_TRANSACTIONS)]
        public async Task<Transaction> NewTransactionAsync(
            NewTransactionRequest input,
            [Service] ITransactionService transactionService
        )
        {
            var transaction = await transactionService.PostAsync(
                input.ShareId,
                input.Amount,
                input.Comment,
                takeNegative: input.TakeNegative ?? false,
                withdrawalLimit: false
            );

            return transaction;
        }

        /// <summary>
        ///     Perform a bulk transaction and return transaction information.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="transactionService"></param>
        /// <param name="skipBelowNegative">
        ///     Continue posting other transactions if a withdrawal cannot be completed due to nonsufficient funds
        ///     while the <c>takeNegative</c> flag is set to <see langword="false" />.
        /// </param>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_TRANSACTIONS)]
        public async Task<IQueryable<Transaction>> NewBulkTransactionAsync(
            IEnumerable<NewTransactionRequest> input,
            [Service] ITransactionService transactionService,
            bool skipBelowNegative = false
        )
        {
            var transactions = await transactionService.PostAsync(
                input,
                !skipBelowNegative,
                false
            );

            return transactions;
        }

        /// <summary>
        ///     Transfer funds from one share to another, if authorized.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <param name="transactionService"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), Authorize]
        public async Task<Tuple<Transaction, Transaction>> NewTransferAsync(
            NewTransferRequest input,
            [ScopedService] AppDbContext context,
            [Service] ITransactionService transactionService,
            [Service] IResolverContext resolverContext
        )
        {
            // Fetch the student ID that owns the source share and validate they are authorized
            long? studentId = await context.Shares
                    .Where(x => x.Id == input.SourceShareId)
                    .Select(x => (long?) x.StudentId)
                    .FirstOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            resolverContext.SetUser(studentId.Value, Constants.UserType.Student);
            var auth = await resolverContext.AuthorizeAsync(
                $"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManageStudents}>"
            );

            if (!auth.Succeeded) {
                throw ErrorFactory.Unauthorized();
            }

            var transactions = await transactionService.TransferAsync(
                input.SourceShareId,
                input.DestinationShareId,
                input.Amount,
                input.Comment,
                withdrawalLimit: resolverContext.GetUserType() != Constants.UserType.User
            );

            return transactions.ToTuple();
        }

        /// <summary>
        ///     Post dividends for a specific <see cref="dal.Entities.ShareType" /> and a group
        ///     of <see cref="dal.Entities.Instance" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="transactionService"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.Privilege.PRIVILEGE_MANAGE_TRANSACTIONS)]
        public async Task<bool> PostDividendsAsync(
            PostDividendsRequest input,
            [Service] ITransactionService transactionService
        )
        {
            try {
                await transactionService.PostDividendsAsync(input);
            } catch (Exception e) {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return true;
        }
    }
}
