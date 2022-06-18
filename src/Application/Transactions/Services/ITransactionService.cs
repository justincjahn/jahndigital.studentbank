using JahnDigital.StudentBank.Application.Transactions.DTOs;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Exceptions;

namespace JahnDigital.StudentBank.Application.Transactions.Services;

/// <summary>
///     Contract for posting monetary transactions and purchasing stocks/products.
/// </summary>
public interface ITransactionService
{
    /// <summary>
    ///     Post a transaction, or throw an error.
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="NonsufficientFundsException">
    ///     If the share doesn't have enough funds. NSF transactions are posted before being thrown.
    /// </exception>
    /// <exception cref="WithdrawalLimitExceededException">
    ///     If the Share has exceeded its withdrawal limit for the period and the Share Type isn't configured to fee.
    /// </exception>
    Task<Transaction> PostAsync(TransactionRequest transaction, CancellationToken cancellationToken = new());

    /// <summary>
    ///     Post several transactions in bulk, or throw an error.  Transactions are posted as a
    ///     database transaction, which is rolled back if an error occurs.  Shares that throw a
    ///     <see cref="NonsufficientFundsException" /> can be skipped by setting <paramref name="stopOnException" />
    ///     to <see langword="false" />, allowing other transactions to post successfully.
    /// </summary>
    /// <param name="transactions"></param>
    /// <param name="stopOnException">
    ///     If the post should stop and revert if a <see cref="NonsufficientFundsException" />
    ///     occurs.
    /// </param>
    /// <param name="withdrawalLimit">If the withdrawal limit should be assessed on this transaction.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The resulting transaction objects.</returns>
    /// <exception cref="NonsufficientFundsException">If the share doesn't have enough funds.</exception>
    /// <exception cref="WithdrawalLimitExceededException">
    ///     If the Share has exceeded its withdrawal limit for the period and the Share Type isn't configured to fee.
    /// </exception>
    Task<IQueryable<Transaction>> PostAsync(
        IEnumerable<TransactionRequest> transactions,
        bool stopOnException = true,
        bool withdrawalLimit = true,
        CancellationToken cancellationToken = new()
    );

    /// <summary>
    ///     Transfer funds from one share to another.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A tuple with the source and destination transactions respectively.</returns>
    Task<(Transaction, Transaction)> TransferAsync(TransferRequest request, CancellationToken cancellationToken = new());

    /// <summary>
    ///     Attempt to make a purchase.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NonsufficientFundsException">If the share doesn't have enough funds.</exception>
    /// <exception cref="InvalidQuantityException">If a product doesn't have stock to fulfill the requested quantity.</exception>
    Task<StudentPurchase> PurchaseAsync(PurchaseRequest input, CancellationToken cancellationToken = new());

    /// <summary>
    ///     Attempt to buy or sell shares of a stock.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The ID number of the <see cref="StudentStock" /> item created.</returns>
    Task<StudentStock> PurchaseStockAsync(PurchaseStockRequest input, CancellationToken cancellationToken = new());

    /// <summary>
    ///     Post dividends for a specific <see cref="ShareType" /> on a specific
    ///     set of <see cref="Instance" />.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> PostDividendsAsync(PostDividendsRequest input, CancellationToken cancellationToken = new());
}
