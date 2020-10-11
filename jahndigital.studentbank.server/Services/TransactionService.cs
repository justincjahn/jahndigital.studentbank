using System;
using System.Linq;
using System.Threading.Tasks;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.server.Exceptions;
using jahndigital.studentbank.server.Extensions;
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
        private AppDbContext _context;

        public TransactionService(AppDbContext context) => _context = context;

        /// <inheritdoc />
        /// <exception cref="NonsufficientFundsException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task PostAsync(
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

            if (share.Balance < amount && !takeNegative) {
                throw new NonsufficientFundsException(share, amount);
            }

            share.Balance += amount;
            share.DateLastActive = DateTime.UtcNow;

            _context.Add(new dal.Entities.Transaction {
                Amount = amount,
                NewBalance = share.Balance,
                TargetShare = share,
                TransactionType = type,
                EffectiveDate = effectiveDate ?? DateTime.UtcNow,
                Comment = comment ?? string.Empty
            });

            try {
                await _context.SaveChangesAsync();
            } catch (Exception e) {
                throw new DatabaseException(e.Message);
            }
        }

        /// <inheritdoc />
        /// <exception cref="ShareNotFoundException">If either share isn't found.</exception>
        /// <exception cref="NonsufficientFundsException">If the source doesn't have the funds needed to transfer.</exception>
        /// <exception cref="DatabaseException">If a database error occurs.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the amount is not a positive value.</exception>
        public async Task TransferAsync(
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
                .Where(x => x.Id == sourceShareId && x.Student.DateDeleted == null)
                .FirstOrDefaultAsync()
            ?? throw new ShareNotFoundException(sourceShareId);

            var destinationShare = await _context.Shares
                .Include(x => x.Student)
                .Where(x => x.Id == destinationShareId && x.Student.DateDeleted == null)
                .FirstOrDefaultAsync()
            ?? throw new ShareNotFoundException(destinationShareId);

            if (sourceShare.Balance < amount && !takeNegative) {
                throw new NonsufficientFundsException(sourceShare, amount);
            }

            sourceShare.Balance -= amount;
            sourceShare.DateLastActive = DateTime.UtcNow;
            destinationShare.Balance += amount;
            destinationShare.DateLastActive = DateTime.UtcNow;

            var tranComment = $"Transfer to #{destinationShare.Student.AccountNumber}S{destinationShare.Id}";
            if (comment != null) tranComment += $". Comment: {comment}";
            _context.Add(new dal.Entities.Transaction {
                Amount = -amount,
                NewBalance = sourceShare.Balance,
                TargetShare = sourceShare,
                TransactionType = "T",
                EffectiveDate = effectiveDate ?? DateTime.UtcNow,
                Comment = tranComment
            });

            tranComment = $"Transfer from #{sourceShare.Student.AccountNumber}";
            if (comment != null) tranComment += $". Comment: {comment}";
            _context.Add(new dal.Entities.Transaction {
                Amount = amount,
                NewBalance = destinationShare.Balance,
                TargetShare = destinationShare,
                TransactionType = "T",
                EffectiveDate = effectiveDate ?? DateTime.UtcNow,
                Comment = tranComment
            });

            try {
                await _context.SaveChangesAsync();
            } catch (Exception e) {
                throw new DatabaseException(e.Message);
            }
        }
    
        /// <inheritdoc />
        /// <exception cref="InvalidQuantityException">If the requested quantity exceeds inventory.</exception>
        /// <exception cref="NonsufficientFundsException">If the share does not have sufficient funds to make the purchase.</exception>
        /// <exception cref="UnauthorizedPurchaseException">If an item being purchased is not assigned to the student's group.</exception>
        /// <exception cref="AggregateException">If there were multiple database errors during the transaction.</exception>
        /// <exception cref="StudentNotFoundException">If the student provided in the input was not found.</exception>
        /// <exception cref="DatabaseException">If a database error occurs.</exception>
        public async Task<long> PurchaseAsync(PurchaseRequest input)
        {
            // Validate that the items don't contain negative or zero quantities
            if(input.Items.Any(x => x.Count < 1)) {
                throw new ArgumentOutOfRangeException("Purchase quantities must be greater than zero.");
            }

            // Get the share's Student ID and Group ID for later use
            var share = await _context.Shares
                .Include(x => x.Student)
                    .ThenInclude(x => x.Group)
                .Where(x =>
                    x.Id == input.ShareId
                    && x.Student.DateDeleted == null
                    && x.Student.Group.DateDeleted == null)
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

            return purchase.Id;
        }
    }
}
