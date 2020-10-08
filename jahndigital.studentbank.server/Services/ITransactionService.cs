using System;
using System.Threading.Tasks;
using jahndigital.studentbank.server.Exceptions;
using jahndigital.studentbank.server.Models;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.server.Services
{
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
        /// <returns></returns>
        /// <exception cref="NonsufficientFundsException">If the share doesn't have enough funds.</exception>
        Task PostAsync(
            long shareId,
            Money amount,
            string? comment = null,
            string? type = null,
            DateTime? effectiveDate = null,
            bool takeNegative = false
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
        /// <returns></returns>
        Task TransferAsync(
            long sourceShareId,
            long destinationShareId,
            Money amount,
            string? comment = null,
            DateTime? effectiveDate = null,
            bool takeNegative = false
        );

        /// <summary>
        /// Attempt to make a purchase.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="NonsufficientFundsException">If the share doesn't have enough funds.</exception>
        /// <exception cref="InvalidQuantityException">If a product doesn't have stock to fulfill the requested quantity.</exception>
        Task<long> PurchaseAsync(PurchaseRequest input);
    }
}
