using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jahndigital.studentbank.services.DTOs;
using jahndigital.studentbank.services.Exceptions;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.services.Interfaces
{
    /// <summary>
    /// Contract for posting monetary transactions and purchasing stocks/products.
    /// </summary>
    public interface ITransactionService
    {
        /// <summary>
        /// Post a transaction, or throw an error.
        /// </summary>
        /// <param name="shareId"></param>
        /// <param name="amount"></param>
        /// <param name="type"></param>
        /// <param name="comment"></param>
        /// <param name="effectiveDate"></param>
        /// <param name="takeNegative"></param>
        /// <returns>The resulting transaction object.</returns>
        /// <param name="withdrawalLimit">If the withdrawal limit should be assessed on this transaction.</param>
        /// <exception cref="NonsufficientFundsException">
        /// If the share doesn't have enough funds. NSF transactions are posted before being thrown.
        /// </exception>
        /// <exception cref="WithdrawalLimitExceededException">
        /// If the Share has exceeded its withdrawal limit for the period and the Share Type isn't configured to fee.
        /// </exception>
        Task<dal.Entities.Transaction> PostAsync(
            long shareId,
            Money amount,
            string? comment = null,
            string? type = null,
            DateTime? effectiveDate = null,
            bool takeNegative = false,
            bool withdrawalLimit = true
        );

        /// <summary>
        /// Post several transactions in bulk, or throw an error.  Transactions are posted as a
        /// database transaction, which is rolled back if an error occurs.  Shares that throw a
        /// <see cref="NonsufficientFundsException"/> can be skipped by setting <paramref name="stopOnException"/>
        /// to <see langword="false"/>, allowing other transactions to post successfully.
        /// </summary>
        /// <param name="transactions"></param>
        /// <param name="stopOnException">If the post should stop and revert if a <see cref="NonsufficientFundsException"/> occurs.</param>
        /// <param name="withdrawalLimit">If the withdrawal limit should be assessed on this transaction.</param>
        /// <returns>The resulting transaction objects.</returns>
        /// <exception cref="NonsufficientFundsException">If the share doesn't have enough funds.</exception>
        /// <exception cref="DatabaseException">If a database error occurs.</exception>
        /// <exception cref="WithdrawalLimitExceededException">
        /// If the Share has exceeded its withdrawal limit for the period and the Share Type isn't configured to fee.
        /// </exception>
        Task<IQueryable<dal.Entities.Transaction>> PostAsync(
            IEnumerable<NewTransactionRequest> transactions,
            bool stopOnException = true,
            bool withdrawalLimit = true
        );

        /// <summary>
        /// Transfer funds from one share to another.
        /// </summary>
        /// <param name="sourceShareId">The share to withdraw from.</param>
        /// <param name="destinationShareId">The share to deposit into.</param>
        /// <param name="amount">A positive monetary value to transfer.</param>
        /// <param name="comment">An optional transaction comment.</param>
        /// <param name="effectiveDate">The date the transaction was effective.</param>
        /// <param name="takeNegative">If the source share can be taken negative as a result of this transaction.</param>
        /// <param name="withdrawalLimit">If the withdrawal limit should be assessed on this transaction.</param>
        /// <returns>A tuple with the source and destination transactions respectively.</returns>
        Task<(dal.Entities.Transaction, dal.Entities.Transaction)> TransferAsync(
            long sourceShareId,
            long destinationShareId,
            Money amount,
            string? comment = null,
            DateTime? effectiveDate = null,
            bool takeNegative = false,
            bool withdrawalLimit = true
        );

        /// <summary>
        /// Attempt to make a purchase.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="NonsufficientFundsException">If the share doesn't have enough funds.</exception>
        /// <exception cref="InvalidQuantityException">If a product doesn't have stock to fulfill the requested quantity.</exception>
        Task<dal.Entities.StudentPurchase> PurchaseAsync(PurchaseRequest input);

        /// <summary>
        /// Attempt to buy or sell shares of a stock.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>The ID number of the <see cref="jahndigital.studentbank.dal.Entities.StudentStock"/> item created.</returns>
        Task<dal.Entities.StudentStock> PurchaseStockAsync(PurchaseStockRequest input);

        /// <summary>
        /// Post dividends for a specific <see cref="jahndigital.studentbank.dal.Entities.ShareType"/> on a specific
        /// set of <see cref="jahndigital.studentbank.dal.Entities.Instance"/>.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<bool> PostDividendsAsync(PostDividendsRequest input);
    }
}
