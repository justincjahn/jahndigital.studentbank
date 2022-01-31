using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.services.DTOs;
using jahndigital.studentbank.services.Exceptions;
using jahndigital.studentbank.services.Interfaces;
using jahndigital.studentbank.utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace jahndigital.studentbank.services
{
    /// <summary>
    ///     Implementation of <see cref="jahndigital.studentbank.services.Interfaces.ITransactionService" /> that uses EF Core.
    /// </summary>
    public class TransactionService : ITransactionService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        public TransactionService(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Post a transaction using the provided database context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="shareId"></param>
        /// <param name="amount"></param>
        /// <param name="comment"></param>
        /// <param name="type"></param>
        /// <param name="effectiveDate"></param>
        /// <param name="takeNegative"></param>
        /// <param name="withdrawalLimit"></param>
        /// <returns></returns>
        /// <exception cref="ShareNotFoundException"></exception>
        /// <exception cref="NonsufficientFundsException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task<Transaction> PostAsync(
            AppDbContext context,
            long shareId,
            Money amount,
            string? comment = null,
            string? type = null,
            DateTime? effectiveDate = null,
            bool takeNegative = false,
            bool withdrawalLimit = true
        )
        {
            if (type == null)
            {
                if (amount == Money.FromCurrency(0.0m))
                {
                    type = "C";
                }
                else
                {
                    type = amount > Money.FromCurrency(0.0m) ? "D" : "W";
                }
            }

            Share? share = await context.Shares
                    .Include(x => x.Student)
                    .Include(x => x.ShareType)
                    .Where(x => x.Id == shareId && x.Student.DateDeleted == null)
                    .FirstOrDefaultAsync()
                ?? throw new ShareNotFoundException(shareId);

            if (withdrawalLimit && type == "W")
            {
                _assessWithdrawalLimit(share, context);
            }

            if (amount < Money.FromCurrency(0.0m) && share.Balance < amount && !takeNegative)
            {
                NonsufficientFundsException? exception = new NonsufficientFundsException(share, amount);
                context.Add(exception.Transaction);

                try
                {
                    await context.SaveChangesAsync();
                }
                finally
                {
                    throw exception;
                }
            }

            share.Balance += amount;
            share.DateLastActive = DateTime.UtcNow;

            Transaction? transaction = new Transaction
            {
                Amount = amount,
                NewBalance = share.Balance,
                TargetShare = share,
                TransactionType = type,
                EffectiveDate = effectiveDate ?? DateTime.UtcNow,
                Comment = comment ?? string.Empty
            };

            context.Add(transaction);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new DatabaseException(e.Message);
            }

            return transaction;
        }

        /// <inheritdoc />
        /// <exception cref="WithdrawalLimitExceededException"></exception>
        /// <exception cref="NonsufficientFundsException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task<Transaction> PostAsync(
            long shareId,
            Money amount,
            string? comment = null,
            string? type = null,
            DateTime? effectiveDate = null,
            bool takeNegative = false,
            bool withdrawalLimit = true
        )
        {
            await using AppDbContext? context = _factory.CreateDbContext();

            return await PostAsync(
                context,
                shareId,
                amount,
                comment,
                type,
                effectiveDate,
                takeNegative,
                withdrawalLimit);
        }

        /// <inheritdoc />
        /// <exception cref="WithdrawalLimitExceededException"></exception>
        /// <exception cref="NonsufficientFundsException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task<IQueryable<Transaction>> PostAsync(
            IEnumerable<NewTransactionRequest> transactions,
            bool stopOnException = true,
            bool withdrawalLimit = true
        )
        {
            await using AppDbContext? context = _factory.CreateDbContext();
            List<Transaction>? postedTransactions = new List<Transaction>();
            IDbContextTransaction? dbTransaction = await context.Database.BeginTransactionAsync();

            foreach (NewTransactionRequest? transaction in transactions)
            {
                try
                {
                    postedTransactions.Add(await PostAsync(
                        context,
                        transaction.ShareId,
                        transaction.Amount,
                        transaction.Comment,
                        takeNegative: transaction.TakeNegative ?? false,
                        withdrawalLimit: withdrawalLimit
                    ));
                }
                catch (NonsufficientFundsException e)
                {
                    if (stopOnException)
                    {
                        await dbTransaction.RollbackAsync();

                        // TODO: Log this exception instead of just re-throwing it.
                        throw;
                    }

                    postedTransactions.Add(e.Transaction);
                }
                catch (DatabaseException)
                {
                    await dbTransaction.RollbackAsync();

                    // TODO: Log this exception instead of just re-throwing it.
                    throw;
                }
                catch (Exception e)
                {
                    await dbTransaction.RollbackAsync();

                    throw new DatabaseException(e.Message);
                }
            }

            try
            {
                await dbTransaction.CommitAsync();
            }
            catch (Exception e)
            {
                throw new DatabaseException(e.Message);
            }

            return postedTransactions.AsQueryable();
        }

        /// <inheritdoc />
        /// <exception cref="WithdrawalLimitExceededException"></exception>
        /// <exception cref="ShareNotFoundException">If either share isn't found.</exception>
        /// <exception cref="NonsufficientFundsException">If the source doesn't have the funds needed to transfer.</exception>
        /// <exception cref="DatabaseException">If a database error occurs.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     If the amount is not a positive value or the destination share isn't in
        ///     the same instance as the source.
        /// </exception>
        public async Task<(Transaction, Transaction)> TransferAsync(
            long sourceShareId,
            long destinationShareId,
            Money amount,
            string? comment = null,
            DateTime? effectiveDate = null,
            bool takeNegative = false,
            bool withdrawalLimit = true
        )
        {
            await using AppDbContext? context = _factory.CreateDbContext();

            if (amount < Money.FromCurrency(0.0m))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    "Amount must be a positive value."
                );
            }

            Share? sourceShare = await context.Shares
                    .Include(x => x.Student)
                    .ThenInclude(x => x.Group)
                    .Include(x => x.ShareType)
                    .Where(x => x.Id == sourceShareId && x.Student.DateDeleted == null)
                    .FirstOrDefaultAsync()
                ?? throw new ShareNotFoundException(sourceShareId);

            Share? destinationShare = await context.Shares
                    .Include(x => x.Student)
                    .ThenInclude(x => x.Group)
                    .Where(x => x.Id == destinationShareId && x.Student.DateDeleted == null)
                    .FirstOrDefaultAsync()
                ?? throw new ShareNotFoundException(destinationShareId);

            if (sourceShare.Student.Group.InstanceId != destinationShare.Student.Group.InstanceId)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(destinationShareId),
                    "Source and destination share must be in the same instance."
                );
            }

            if (withdrawalLimit)
            {
                _assessWithdrawalLimit(sourceShare, context);
            }

            if (sourceShare.Balance < amount && !takeNegative)
            {
                throw new NonsufficientFundsException(sourceShare, amount);
            }

            sourceShare.Balance -= amount;
            sourceShare.DateLastActive = DateTime.UtcNow;
            destinationShare.Balance += amount;
            destinationShare.DateLastActive = DateTime.UtcNow;

            string? tranComment = $"Transfer to #{destinationShare.Student.AccountNumber}S{destinationShare.Id}";

            if (comment != null)
            {
                tranComment += $". Comment: {comment}";
            }

            Transaction? sourceTransaction = new Transaction
            {
                Amount = -amount,
                NewBalance = sourceShare.Balance,
                TargetShare = sourceShare,
                TransactionType = "T",
                EffectiveDate = effectiveDate ?? DateTime.UtcNow,
                Comment = tranComment
            };

            tranComment = $"Transfer from #{sourceShare.Student.AccountNumber}";

            if (comment != null)
            {
                tranComment += $". Comment: {comment}";
            }

            Transaction? destinationTransaction = new Transaction
            {
                Amount = amount,
                NewBalance = destinationShare.Balance,
                TargetShare = destinationShare,
                TransactionType = "T",
                EffectiveDate = effectiveDate ?? DateTime.UtcNow,
                Comment = tranComment
            };

            context.Add(sourceTransaction);
            context.Add(destinationTransaction);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new DatabaseException(e.Message);
            }

            return (sourceTransaction, destinationTransaction);
        }

        /// <inheritdoc />
        /// <exception cref="WithdrawalLimitExceededException"></exception>
        /// <exception cref="InvalidQuantityException">If the requested quantity exceeds inventory.</exception>
        /// <exception cref="NonsufficientFundsException">If the share does not have sufficient funds to make the purchase.</exception>
        /// <exception cref="UnauthorizedPurchaseException">If an item being purchased is not assigned to the student's group.</exception>
        /// <exception cref="AggregateException">If there were multiple database errors during the transaction.</exception>
        /// <exception cref="StudentNotFoundException">If the student provided in the input was not found.</exception>
        /// <exception cref="DatabaseException">If a database error occurs.</exception>
        public async Task<StudentPurchase> PurchaseAsync(PurchaseRequest input)
        {
            await using AppDbContext? context = _factory.CreateDbContext();

            // Validate that the items don't contain negative or zero quantities
            if (input.Items.Any(x => x.Count < 1))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(input),
                    "Purchase quantities must be greater than zero."
                );
            }

            // Get the share's Student ID and Instance ID for later use
            var share = await context.Shares
                    .Include(x => x.Student)
                    .ThenInclude(x => x.Group)
                    .ThenInclude(x => x.Instance)
                    .Where(x =>
                        x.Id == input.ShareId
                        && x.Student.DateDeleted == null
                        && x.Student.Group.DateDeleted == null
                        && x.Student.Group.Instance.DateDeleted == null)
                    .Select(x => new { x.StudentId, x.Student.Group.InstanceId }).FirstOrDefaultAsync()
                ?? throw new ShareNotFoundException(input.ShareId);

            // Pull the list of requested products to validate cost and quantity
            List<long>? productIds = input.Items.Select(x => x.ProductId).ToList();
            List<Product>? products = await context.ProductInstances
                .Include(x => x.Product)
                .Where(x =>
                    x.InstanceId == share.InstanceId
                    && productIds.Contains(x.ProductId))
                .Select(x => x.Product)
                .ToListAsync();

            StudentPurchase? purchase = new StudentPurchase { StudentId = share.StudentId };

            Money? total = Money.FromCurrency(0.0m);

            foreach (Product? product in products)
            {
                PurchaseRequestItem? purchaseItem = input.Items.First(x => x.ProductId == product.Id);

                if (product.IsLimitedQuantity && purchaseItem.Count > product.Quantity)
                {
                    throw new InvalidQuantityException(product, purchaseItem.Count);
                }

                total += product.Cost * purchaseItem.Count;

                if (product.IsLimitedQuantity)
                {
                    product.Quantity -= purchaseItem.Count;
                }

                purchase.Items.Add(new StudentPurchaseItem
                {
                    Quantity = purchaseItem.Count,
                    PurchasePrice = product.Cost,
                    StudentPurchase = purchase,
                    Product = product
                });
            }

            purchase.TotalCost = total;
            context.Add(purchase);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new DatabaseException(e.Message);
            }

            try
            {
                await PostAsync(input.ShareId, -total, $"Purchase #{purchase.Id}");
            }
            catch (Exception e)
            {
                // Something happened, try to delete the purchase, and restock inventory
                foreach (StudentPurchaseItem? item in purchase.Items)
                {
                    Product? product = products.Find(x => x.Id == item.ProductId) ?? throw new Exception();

                    if (product.IsLimitedQuantity)
                    {
                        product.Quantity += item.Quantity;
                    }

                    context.Remove(item);
                }

                context.Remove(purchase);

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (Exception e2)
                {
                    throw new AggregateException(e, e2);
                }

                // TODO: Log this exception instead of just re-throwing it.
                throw;
            }

            return purchase;
        }

        /// <inheritdoc />
        /// <exception cref="WithdrawalLimitExceededException"></exception>
        /// <exception cref="InvalidShareQuantityException">If the requested shares exceed available shares.</exception>
        /// <exception cref="NonsufficientFundsException">If the share does not have sufficient funds to make the purchase.</exception>
        /// <exception cref="UnauthorizedPurchaseException">
        ///     If the shares being purchased belong to a stock the student doesn't
        ///     have access to.
        /// </exception>
        /// <exception cref="StudentNotFoundException">If the student provided in the input was not found.</exception>
        /// <exception cref="StockNotFoundException">
        ///     If the stock provided in the input as not found or isn't linked to the
        ///     instance the share belongs to.
        /// </exception>
        /// <exception cref="DatabaseException">If a database error occurs.</exception>
        public async Task<StudentStock> PurchaseStockAsync(PurchaseStockRequest input)
        {
            await using AppDbContext? context = _factory.CreateDbContext();

            bool stockExists = await context.Stocks.Where(x => x.Id == input.StockId).AnyAsync();

            if (!stockExists)
            {
                throw new StockNotFoundException(input.StockId);
            }

            // Get the share's Student ID and Instance ID for later use
            var share = await context.Shares
                    .Include(x => x.Student)
                    .ThenInclude(x => x.Group)
                    .ThenInclude(x => x.Instance)
                    .Where(x =>
                        x.Id == input.ShareId
                        && x.Student.DateDeleted == null
                        && x.Student.Group.DateDeleted == null
                        && x.Student.Group.Instance.DateDeleted == null)
                    .Select(x => new { x.Id, x.StudentId, x.Student.Group.InstanceId })
                    .SingleOrDefaultAsync()
                ?? throw new ShareNotFoundException(input.ShareId);

            // Get the stock provided it is included in the instance of the share
            Stock? stock = await context.Stocks
                    .Include(x => x.StockInstances)
                    .Where(x =>
                        x.Id == input.StockId
                        && x.StockInstances.Any(xx => xx.InstanceId == share.InstanceId))
                    .SingleOrDefaultAsync()
                ?? throw new UnauthorizedPurchaseException();

            // Ensure there are enough shares available to purchase
            if (stock.AvailableShares < input.Quantity)
            {
                throw new InvalidShareQuantityException(stock, input.Quantity);
            }

            // Fetch or create the StudentStock entity
            StudentStock? studentStock = await context.StudentStocks
                .Where(x => x.StudentId == share.StudentId && x.StockId == stock.Id)
                .SingleOrDefaultAsync();

            if (studentStock == null)
            {
                studentStock = new StudentStock
                {
                    StudentId = share.StudentId,
                    StockId = stock.Id,
                    SharesOwned = 0,
                    NetContribution = Money.FromCurrency(0m)
                };

                context.Add(studentStock);
            }

            // If the student is selling stock, make sure they have enough shares
            if (input.Quantity < 0 && studentStock.SharesOwned < -input.Quantity)
            {
                throw new InvalidShareQuantityException(stock, input.Quantity);
            }

            studentStock.SharesOwned += input.Quantity;
            studentStock.DateLastActive = DateTime.UtcNow;

            Money? totalCost = stock.CurrentValue * input.Quantity * -1;
            string? buySell = totalCost.Amount > 0.0M ? "sale" : "purchase";
            Transaction? transaction = await PostAsync(
                share.Id,
                totalCost,
                $"Stock {buySell}: {input.Quantity} shares of {stock.Symbol} at {stock.CurrentValue}"
            );

            studentStock.History.Add(new StudentStockHistory
            {
                Amount = totalCost, Count = input.Quantity, StudentStock = studentStock, Transaction = transaction
            });

            stock.AvailableShares -= input.Quantity;
            studentStock.NetContribution += transaction.Amount * -1;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                try
                {
                    await PostAsync(share.Id, -totalCost,
                        $"VOIDED: Stock purchase: {input.Quantity} shares of {stock.Symbol}");
                }
                catch (Exception e2)
                {
                    throw new AggregateException(e, e2);
                }

                // TODO: Log this exception instead of just re-throwing it.
                throw;
            }

            return studentStock;
        }

        /// <inheritdoc />
        /// <exception cref="ShareTypeNotFoundException">If the share type was not found.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If any instance(s) or shareTypes specified were invalid.</exception>
        /// <exception cref="DatabaseException">If any database error(s) occurred during the posting.</exception>
        public async Task<bool> PostDividendsAsync(PostDividendsRequest input)
        {
            await using AppDbContext? context = _factory.CreateDbContext();

            ShareType? shareType = await context.ShareTypes
                    .Where(x => x.Id == input.ShareTypeId)
                    .SingleOrDefaultAsync()
                ?? throw new ShareTypeNotFoundException(input.ShareTypeId);

            if (shareType.DividendRate == Rate.FromRate(0.0m))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(input),
                    "Share Type provided has no dividend rate."
                );
            }

            int instances = await context.Instances
                .Where(x => input.Instances.Contains(x.Id))
                .CountAsync();

            if (instances != input.Instances.Count())
            {
                throw new ArgumentOutOfRangeException(
                    nameof(input),
                    "One or more instances provided do not exist."
                );
            }

            IDbContextTransaction? transaction = await context.Database.BeginTransactionAsync();

            foreach (long instance in input.Instances)
            {
                IQueryable<Share>? query = context.Shares
                    .Include(x => x.Student)
                    .ThenInclude(x => x.Group)
                    .Where(x =>
                        x.ShareTypeId == shareType.Id
                        && x.Student.Group.InstanceId == instance
                        && x.RawBalance > 0);

                int count = await query.CountAsync();

                for (int i = 0; i < count; i += 100)
                {
                    List<Share>? shares = await query.Skip(i).Take(100).ToListAsync();

                    foreach (Share? share in shares)
                    {
                        Money? dividendAmount = share.Balance * shareType.DividendRate;
                        Money? newBalance = share.Balance += dividendAmount;

                        context.Add(new Transaction
                        {
                            Amount = dividendAmount,
                            EffectiveDate = DateTime.UtcNow,
                            NewBalance = newBalance,
                            TargetShareId = share.Id,
                            TransactionType = "V",
                            Comment = $"Dividend posting {shareType.DividendRate}"
                        });

                        share.DividendLastAmount = dividendAmount;
                        share.TotalDividends += dividendAmount;
                        share.Balance = newBalance;
                    }

                    try
                    {
                        await context.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        throw new DatabaseException(e.Message);
                    }
                }
            }

            try
            {
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                throw new DatabaseException(e.Message);
            }

            return true;
        }

        /// <summary>
        ///     Assess the withdrawal limit for the transaction.
        /// </summary>
        /// <param name="share"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private void _assessWithdrawalLimit(Share share, AppDbContext context)
        {
            ShareType? shareType = share.ShareType;

            if (shareType.WithdrawalLimitCount <= 0)
            {
                return;
            }

            if (share.LimitedWithdrawalCount >= shareType.WithdrawalLimitCount &&
                !shareType.WithdrawalLimitShouldFee)
            {
                throw new WithdrawalLimitExceededException(shareType, share);
            }

            if (share.LimitedWithdrawalCount >= shareType.WithdrawalLimitCount &&
                shareType.WithdrawalLimitShouldFee)
            {
                // Charge a fee for the withdrawal instead of denying it
                share.Balance -= shareType.WithdrawalLimitFee;

                context.Add(new Transaction
                {
                    Amount = shareType.WithdrawalLimitFee,
                    NewBalance = share.Balance,
                    TargetShare = share,
                    TransactionType = "F",
                    EffectiveDate = DateTime.UtcNow,
                    Comment = "Withdrawal Limit Fee"
                });
            }

            share.LimitedWithdrawalCount += 1;
        }
    }
}
