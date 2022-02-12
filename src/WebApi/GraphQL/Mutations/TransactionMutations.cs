using System;
using System.Collections.Generic;
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
using JahnDigital.StudentBank.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
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
        [HotChocolate.AspNetCore.Authorization.Authorize(Policy = Privilege.PRIVILEGE_MANAGE_TRANSACTIONS)]
        public async Task<Transaction> NewTransactionAsync(
            TransactionRequest input,
            [Service] ITransactionService transactionService
        )
        {
            var transaction = await transactionService.PostAsync(input);
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
        [HotChocolate.AspNetCore.Authorization.Authorize(Policy = Privilege.PRIVILEGE_MANAGE_TRANSACTIONS)]
        public async Task<IQueryable<Transaction>> NewBulkTransactionAsync(
            IEnumerable<NewTransactionRequest> input,
            [Service] ITransactionService transactionService,
            bool skipBelowNegative = false
        )
        {
            var transactions = input.Select(x => new TransactionRequest()
            {
                Amount = x.Amount,
                Comment = x.Comment,
                ShareId = x.ShareId,
                TakeNegative = x.TakeNegative
            });
            
            var result = await transactionService.PostAsync(transactions, !skipBelowNegative, false);
            return result;
        }

        /// <summary>
        ///     Transfer funds from one share to another, if authorized.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <param name="transactionService"></param>
        /// <param name="resolverContext"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), HotChocolate.AspNetCore.Authorization.Authorize]
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
                    .Select(x => (long?)x.StudentId)
                    .FirstOrDefaultAsync()
                ?? throw ErrorFactory.NotFound();

            resolverContext.SetUser(studentId.Value, UserType.Student);
            AuthorizationResult? auth = await resolverContext.AuthorizeAsync(
                $"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageStudents}>"
            );

            if (!auth.Succeeded)
            {
                throw ErrorFactory.Unauthorized();
            }

            var transactions = await transactionService.TransferAsync(new TransferRequest()
            {
                Amount = input.Amount,
                Comment = input.Comment,
                WithdrawalLimit = resolverContext.GetUserType() != UserType.User,
                DestinationShareId = input.DestinationShareId,
                SourceShareId = input.SourceShareId
            });

            return transactions.ToTuple();
        }

        /// <summary>
        ///     Post dividends for a specific <see cref="ShareType" /> and a group
        ///     of <see cref="Instance" />.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="transactionService"></param>
        /// <returns></returns>
        [HotChocolate.AspNetCore.Authorization.Authorize(Policy = Privilege.PRIVILEGE_MANAGE_TRANSACTIONS)]
        public async Task<bool> PostDividendsAsync(
            PostDividendsRequest input,
            [Service] ITransactionService transactionService
        )
        {
            try
            {
                await transactionService.PostDividendsAsync(input);
            }
            catch (Exception e)
            {
                throw ErrorFactory.QueryFailed(e.Message);
            }

            return true;
        }
    }
}
