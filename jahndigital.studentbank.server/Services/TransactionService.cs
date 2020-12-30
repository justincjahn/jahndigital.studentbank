using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.server.Exceptions;
using jahndigital.studentbank.server.GraphQL;
using jahndigital.studentbank.server.Models;
using jahndigital.studentbank.utils;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.server.Services
{
    /// <summary>
    /// Implementation of <see cref="ITransactionService"/> that uses EF Core.
    /// </summary>
    public class TransactionService : ITransactionService
    {
        /// <summary>
        /// The database context to use when querying and updating the data store.
        /// </summary>
        private AppDbContext _context;

        /// <summary>
        /// Initialize the instance with the database context.
        /// </summary>
        /// <param name="context"></param>
        public TransactionService(AppDbContext context) => _context = context;

        /// <inheritdoc />
        /// <exception cref="NonsufficientFundsException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task<dal.Entities.Transaction> PostAsync(
            long shareId,
            Money amount,
            string? comment = null,
            string? type = null,
            DateTime? effectiveDate = null,
            bool takeNegative = false
        ) {
            if (type == null) {
                if (amount == Money.FromCurrency(0.0m)) {
                    type = "C";
                } else {
                    type = amount > Money.FromCurrency(0.0m) ? "D" : "W";
                }
            }

            var share = await _context.Shares
                .Include(x => x.Student)
                .Where(x => x.Id == shareId && x.Student.DateDeleted == null)
                .FirstOrDefaultAsync()
            ?? throw ErrorFactory.NotFound();

            if (amount < Money.FromCurrency(0.0m) && share.Balance < amount && !takeNegative) {
                var exception = new NonsufficientFundsException(share, amount);
                _context.Add(exception.Transaction);

                try {
                    await _context.SaveChangesAsync();
                } finally {
                    throw exception;
                }
            }

            share.Balance += amount;
            share.DateLastActive = DateTime.UtcNow;

            var transaction = new dal.Entities.Transaction {
                Amount = amount,
                NewBalance = share.Balance,
                TargetShare = share,
                TransactionType = type,
                EffectiveDate = effectiveDate ?? DateTime.UtcNow,
                Comment = comment ?? string.Empty
            };

            _context.Add(transaction);

            try {
                await _context.SaveChangesAsync();
            } catch (Exception e) {
                throw new DatabaseException(e.Message);
            }

            return transaction;
        }

        /// <inheritdoc />
        /// <exception cref="NonsufficientFundsException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task<IQueryable<dal.Entities.Transaction>> PostAsync(
            IEnumerable<NewTransactionRequest> transactions,
            bool stopOnException = true
        ) {
            var postedTransactions = new List<dal.Entities.Transaction>();
            var dbTransaction = await _context.Database.BeginTransactionAsync();

            foreach (var transaction in transactions) {
                try {
                    postedTransactions.Add(await PostAsync(
                        transaction.ShareId,
                        transaction.Amount,
                        transaction.Comment,
                        takeNegative: transaction.TakeNegative ?? false
                    ));
                } catch (NonsufficientFundsException e) {
                    if (stopOnException) {
                        await dbTransaction.RollbackAsync();
                        throw e;
                    } else {
                        postedTransactions.Add(e.Transaction);
                    }
                } catch (DatabaseException e) {
                    await dbTransaction.RollbackAsync();
                    throw e;
                } catch (Exception e) {
                    await dbTransaction.RollbackAsync();
                    throw new DatabaseException(e.Message);
                }
            }

            try {
                await dbTransaction.CommitAsync();
            } catch (Exception e) {
                throw new DatabaseException(e.Message);
            }

            return postedTransactions.AsQueryable();
        }

        /// <inheritdoc />
        /// <exception cref="ShareNotFoundException">If either share isn't found.</exception>
        /// <exception cref="NonsufficientFundsException">If the source doesn't have the funds needed to transfer.</exception>
        /// <exception cref="DatabaseException">If a database error occurs.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the amount is not a positive value or the destination share isn't in the same instance as the source.</exception>
        public async Task<(dal.Entities.Transaction, dal.Entities.Transaction)> TransferAsync(
            long sourceShareId,
            long destinationShareId,
            Money amount,
            string? comment = null,
            DateTime? effectiveDate = null,
            bool takeNegative = false
        ) {
            if (amount < Money.FromCurrency(0.0m)) {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    "Amount must be a positive value."
                );
            }

            var sourceShare = await _context.Shares
                .Include(x => x.Student)
                    .ThenInclude(x => x.Group)
                .Where(x => x.Id == sourceShareId && x.Student.DateDeleted == null)
                .FirstOrDefaultAsync()
            ?? throw new ShareNotFoundException(sourceShareId);

            var destinationShare = await _context.Shares
                .Include(x => x.Student)
                    .ThenInclude(x => x.Group)
                .Where(x => x.Id == destinationShareId && x.Student.DateDeleted == null)
                .FirstOrDefaultAsync()
            ?? throw new ShareNotFoundException(destinationShareId);

            if (sourceShare.Student.Group.InstanceId != destinationShare.Student.Group.InstanceId) {
                throw new ArgumentOutOfRangeException(
                    "destinationShare", "Source and destination share must be in the same instance."
                );
            }

            if (sourceShare.Balance < amount && !takeNegative) {
                throw new NonsufficientFundsException(sourceShare, amount);
            }

            sourceShare.Balance -= amount;
            sourceShare.DateLastActive = DateTime.UtcNow;
            destinationShare.Balance += amount;
            destinationShare.DateLastActive = DateTime.UtcNow;

            var tranComment = $"Transfer to #{destinationShare.Student.AccountNumber}S{destinationShare.Id}";
            if (comment != null) tranComment += $". Comment: {comment}";
            var sourceTransaction = new dal.Entities.Transaction {
                Amount = -amount,
                NewBalance = sourceShare.Balance,
                TargetShare = sourceShare,
                TransactionType = "T",
                EffectiveDate = effectiveDate ?? DateTime.UtcNow,
                Comment = tranComment
            };

            tranComment = $"Transfer from #{sourceShare.Student.AccountNumber}";
            if (comment != null) tranComment += $". Comment: {comment}";
            var destinationTransaction = new dal.Entities.Transaction {
                Amount = amount,
                NewBalance = destinationShare.Balance,
                TargetShare = destinationShare,
                TransactionType = "T",
                EffectiveDate = effectiveDate ?? DateTime.UtcNow,
                Comment = tranComment
            };

            _context.Add(sourceTransaction);
            _context.Add(destinationTransaction);

            try {
                await _context.SaveChangesAsync();
            } catch (Exception e) {
                throw new DatabaseException(e.Message);
            }

            return (sourceTransaction, destinationTransaction);
        }

        /// <inheritdoc />
        /// <exception cref="InvalidQuantityException">If the requested quantity exceeds inventory.</exception>
        /// <exception cref="NonsufficientFundsException">If the share does not have sufficient funds to make the purchase.</exception>
        /// <exception cref="UnauthorizedPurchaseException">If an item being purchased is not assigned to the student's group.</exception>
        /// <exception cref="AggregateException">If there were multiple database errors during the transaction.</exception>
        /// <exception cref="StudentNotFoundException">If the student provided in the input was not found.</exception>
        /// <exception cref="DatabaseException">If a database error occurs.</exception>
        public async Task<dal.Entities.StudentPurchase> PurchaseAsync(PurchaseRequest input)
        {
            // Validate that the items don't contain negative or zero quantities
            if(input.Items.Any(x => x.Count < 1)) {
                throw new ArgumentOutOfRangeException("Purchase quantities must be greater than zero.");
            }

            // Get the share's Student ID and Instance ID for later use
            var share = await _context.Shares
                .Include(x => x.Student)
                    .ThenInclude(x => x.Group)
                        .ThenInclude(x => x.Instance)
                .Where(x =>
                    x.Id == input.ShareId
                    && x.Student.DateDeleted == null
                    && x.Student.Group.DateDeleted == null
                    && x.Student.Group.Instance.DateDeleted == null)
                .Select(x => new {
                    StudentId = x.StudentId,
                    InstanceId = x.Student.Group.InstanceId
                }).FirstOrDefaultAsync()
            ?? throw new ShareNotFoundException(input.ShareId);

            // Pull the list of requested products to validate cost and quantity
            var productIds = input.Items.Select(x => x.ProductId).ToList();
            var products = await _context.ProductInstances
                .Include(x => x.Product)
                .Where(x =>
                    x.InstanceId == share.InstanceId
                    && productIds.Contains(x.ProductId))
                .Select(x => x.Product)
                .ToListAsync();

            var purchase = new dal.Entities.StudentPurchase {
                StudentId = share.StudentId
            };

            Money total = Money.FromCurrency(0.0m);
            foreach(var product in products) {
                var purchaseItem = input.Items.First(x => x.ProductId == product.Id);
                if (product.IsLimitedQuantity && purchaseItem.Count > product.Quantity) {
                    throw new InvalidQuantityException(product, purchaseItem.Count);
                }

                total += product.Cost * purchaseItem.Count;
                if (product.IsLimitedQuantity) product.Quantity -= purchaseItem.Count;

                purchase.Items.Add(new dal.Entities.StudentPurchaseItem {
                    Quantity = purchaseItem.Count,
                    PurchasePrice = product.Cost,
                    StudentPurchase = purchase,
                    Product = product
                });
            }

            purchase.TotalCost = total;
            _context.Add(purchase);

            try {
                await _context.SaveChangesAsync();
            } catch (Exception e) {
                throw new DatabaseException(e.Message);
            }

            try {
                await PostAsync(input.ShareId, -total, comment: $"Purchase #{purchase.Id}");
            } catch (Exception e) {
                // Something happened, try to delete the purchase, and restock inventory
                foreach(var item in purchase.Items) {
                    var product = products.Find(x => x.Id == item.ProductId) ?? throw new Exception();
                    if (product.IsLimitedQuantity) product.Quantity += item.Quantity;
                    _context.Remove(item);
                }

                _context.Remove(purchase);

                try {
                    await _context.SaveChangesAsync();
                } catch (Exception e2) {
                    throw new AggregateException(e, e2);
                }

                throw e;
            }

            return purchase;
        }

        /// <inheritdoc />
        /// <exception cref="InvalidShareQuantityException">If the requested shares exceed available shares.</exception>
        /// <exception cref="NonsufficientFundsException">If the share does not have sufficient funds to make the purchase.</exception>
        /// <exception cref="UnauthorizedPurchaseException">If the shares being purchased belong to a stock the student doesn't have access to.</exception>
        /// <exception cref="StudentNotFoundException">If the student provided in the input was not found.</exception>
        /// <exception cref="StockNotFoundException">If the stock provided in the input as not found or isn't linked to the instance the share belongs to.</exception>
        /// <exception cref="DatabaseException">If a database error occurs.</exception>
        public async Task<dal.Entities.StudentStock> PurchaseStockAsync(PurchaseStockRequest input)
        {
            var stockExists = await _context.Stocks.Where(x => x.Id == input.StockId).AnyAsync();
            if (!stockExists) throw new StockNotFoundException(input.StockId);

            // Get the share's Student ID and Instance ID for later use
            var share = await _context.Shares
                .Include(x => x.Student)
                    .ThenInclude(x => x.Group)
                        .ThenInclude(x => x.Instance)
                .Where(x =>
                    x.Id == input.ShareId
                    && x.Student.DateDeleted == null
                    && x.Student.Group.DateDeleted == null
                    && x.Student.Group.Instance.DateDeleted == null)
                .Select(x => new {
                    Id = x.Id,
                    StudentId = x.StudentId,
                    InstanceId = x.Student.Group.InstanceId})
                .SingleOrDefaultAsync()
            ?? throw new ShareNotFoundException(input.ShareId);

            // Get the stock provided it is included in the instance of the share
            var stock = await _context.Stocks
                .Include(x => x.StockInstances)
                .Where(x =>
                    x.Id == input.StockId
                    && x.StockInstances.Any(x => x.InstanceId == share.InstanceId))
                .SingleOrDefaultAsync()
            ?? throw new UnauthorizedPurchaseException();

            // Ensure there are enough shares available to purchase
            if (stock.AvailableShares < input.Quantity) {
                throw new InvalidShareQuantityException(stock, input.Quantity);
            }

            // Fetch or create the StudentStock entity
            var studentStock = await _context.StudentStocks
                .Where(x => x.StudentId == share.StudentId && x.StockId == stock.Id)
                .SingleOrDefaultAsync();

            if (studentStock == null) {
                studentStock = new dal.Entities.StudentStock {
                    StudentId = share.StudentId,
                    StockId = stock.Id,
                    SharesOwned = 0
                };

                _context.Add(studentStock);
            }

            var totalCost = stock.CurrentValue * input.Quantity;

            // If the student is selling stock, make sure they have enough shares
            if (input.Quantity < 0 && studentStock.SharesOwned >= -input.Quantity) {
                throw new InvalidShareQuantityException(stock, input.Quantity);
            }

            dal.Entities.Transaction? transaction;
            try {
                transaction = await PostAsync(share.Id, totalCost, $"Stock purchase: {input.Quantity} shares of {stock.Symbol}");
            } catch (Exception e) {
                throw e;
            }

            studentStock.History.Add(new dal.Entities.StudentStockHistory {
                Amount = totalCost,
                Count = input.Quantity,
                StudentStock = studentStock,
                Transaction = transaction
            });

            stock.AvailableShares -= input.Quantity;

            try {
                await _context.SaveChangesAsync();
            } catch (Exception e) {
                try {
                    await PostAsync(share.Id, -totalCost, $"VOIDED: Stock purchase: {input.Quantity} shares of {stock.Symbol}");
                } catch (Exception e2) {
                    throw new AggregateException(e, e2);
                }

                throw e;
            }

            return studentStock;
        }

        /// <inheritdoc />
        /// <exception cref="ShareTypeNotFoundException">If the share type was not found.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If any instance(s) or shareTypes specified were invalid.</exception>
        /// <exception cref="DatabaseException">If any database error(s) occurred during the posting.</exception>
        public async Task<bool> PostDividendsAsync(PostDividendsRequest input)
        {
            var shareType = await _context.ShareTypes
                .Where(x => x.Id == input.ShareTypeId)
                .SingleOrDefaultAsync()
            ?? throw new ShareTypeNotFoundException(input.ShareTypeId);

            if (shareType.DividendRate == Rate.FromRate(0.0m)) {
                throw new ArgumentOutOfRangeException(
                    "input", "Share Type provided has no dividend rate.");
            }

            var instances = await _context.Instances
                .Where(x => input.Instances.Contains(x.Id))
                .CountAsync();
            
            if (instances != input.Instances.Count()) {
                throw new ArgumentOutOfRangeException(
                    "input", "One or more instances provided do not exist.");
            }

            var transaction = await _context.Database.BeginTransactionAsync();
            foreach (var instance in input.Instances) {
                var query = _context.Shares
                    .Include(x => x.Student)
                        .ThenInclude(x => x.Group)
                    .Where(x =>
                        x.ShareTypeId == shareType.Id
                        && x.Student.Group.InstanceId == instance
                        && x.RawBalance > 0);

                var count = await query.CountAsync();
                for (var i = 0; i < count; i += 100) {
                    var shares = await query.Skip(i).Take(100).ToListAsync();

                    foreach (var share in shares) {
                        var dividendAmount = share.Balance * shareType.DividendRate;
                        var newBalance = share.Balance += dividendAmount;

                        _context.Add(new dal.Entities.Transaction {
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

                    try {
                        await _context.SaveChangesAsync();
                    } catch (Exception e) {
                        throw new DatabaseException(e.Message);
                    }
                }
            }

            try {
                await transaction.CommitAsync();
            } catch (Exception e) {
                throw new DatabaseException(e.Message);
            }

            return true;
        }
    }
}
