using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Application.Transactions.DTOs;
using JahnDigital.StudentBank.Application.Transactions.Services;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Exceptions;
using JahnDigital.StudentBank.Domain.ValueObjects;
using JahnDigital.StudentBank.Infrastructure.Common.Exceptions;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace JahnDigital.StudentBank.Infrastructure.Transactions.Services;

/// <summary>
///     Implementation of <see cref="ITransactionService" /> that uses EF Core.
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly IAppDbContext _context;

    public TransactionService(IAppDbContext context)
    {
        _context = context;
    }

    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ShareNotFoundException"></exception>
    /// <exception cref="NonsufficientFundsException"></exception>
    /// <exception cref="DatabaseException"></exception>
    private async Task<Transaction> PostAsync(TransactionRequest request, IAppDbContext context, CancellationToken cancellationToken)
    {
        string transactionType = request.Type ?? "";
        if (request.Type == null)
        {
            if (request.Amount == Money.FromCurrency(0.0m))
            {
                transactionType = "C";
            }
            else
            {
                transactionType = request.Amount > Money.FromCurrency(0.0m) ? "D" : "W";
            }
        }

        var share = await context.Shares
                .Include(x => x.Student)
                .Include(x => x.ShareType)
                .Where(x => x.Id == request.ShareId && x.Student.DateDeleted == null)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken)
            ?? throw new ShareNotFoundException(request.ShareId);

        if ((request?.WithdrawalLimit ?? true) && transactionType == "W")
        {
            _assessWithdrawalLimit(share, context);
        }

        if ((request?.Amount ?? Money.Zero) < Money.Zero && share.Balance < (request?.Amount ?? Money.Zero) && !(request?.TakeNegative ?? false))
        {
            NonsufficientFundsException? exception = new NonsufficientFundsException(share, request?.Amount ?? Money.Zero);
            context.Transactions.Add(exception.Transaction);

            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                throw exception;
            }
        }

        share.Balance += request?.Amount ?? Money.Zero;
        share.DateLastActive = DateTime.UtcNow;

        Transaction? transaction = new Transaction
        {
            Amount = request?.Amount ?? Money.Zero,
            NewBalance = share.Balance,
            TargetShare = share,
            TransactionType = transactionType,
            EffectiveDate = request?.EffectiveDate ?? DateTime.UtcNow,
            Comment = request?.Comment ?? string.Empty
        };

        context.Transactions.Add(transaction);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
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
    public async Task<Transaction> PostAsync(TransactionRequest request, CancellationToken cancellationToken = new())
    {
        return await PostAsync(request, _context, cancellationToken);
    }

    /// <inheritdoc />
    /// <exception cref="WithdrawalLimitExceededException"></exception>
    /// <exception cref="NonsufficientFundsException"></exception>
    /// <exception cref="DatabaseException"></exception>
    public async Task<IQueryable<Transaction>> PostAsync(
        IEnumerable<TransactionRequest> transactions,
        bool stopOnException = true,
        bool withdrawalLimit = true,
        CancellationToken cancellationToken = new()
    )
    {
        var postedTransactions = new List<Transaction>();
        await using IDbContextTransaction? dbTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        foreach (var transaction in transactions)
        {
            try
            {
                var request = new TransactionRequest()
                {
                    ShareId = transaction.ShareId,
                    Amount = transaction.Amount,
                    Comment = transaction.Comment,
                    TakeNegative = transaction.TakeNegative,
                    WithdrawalLimit = withdrawalLimit
                };

                postedTransactions.Add(await PostAsync(request, _context, cancellationToken));
            }
            catch (NonsufficientFundsException e)
            {
                if (stopOnException)
                {
                    await dbTransaction.RollbackAsync(cancellationToken);

                    // TODO: Log this exception instead of just re-throwing it.
                    throw;
                }

                postedTransactions.Add(e.Transaction);
            }
            catch (DatabaseException)
            {
                await dbTransaction.RollbackAsync(cancellationToken);

                // TODO: Log this exception instead of just re-throwing it.
                throw;
            }
            catch (Exception e)
            {
                await dbTransaction.RollbackAsync(cancellationToken);

                throw new DatabaseException(e.Message);
            }
        }

        try
        {
            await dbTransaction.CommitAsync(cancellationToken);
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
    public async Task<(Transaction, Transaction)> TransferAsync(TransferRequest request, CancellationToken cancellationToken = new())
    {
        if (request.Amount < Money.FromCurrency(0.0m))
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Amount),
                "Amount must be a positive value."
            );
        }

        var sourceShare = await _context.Shares
                .Include(x => x.Student)
                .ThenInclude(x => x.Group)
                .Include(x => x.ShareType)
                .Where(x => x.Id == request.SourceShareId && x.Student.DateDeleted == null)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken)
            ?? throw new ShareNotFoundException(request.SourceShareId);

        var destinationShare = await _context.Shares
                .Include(x => x.Student)
                .ThenInclude(x => x.Group)
                .Where(x => x.Id == request.DestinationShareId && x.Student.DateDeleted == null)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken)
            ?? throw new ShareNotFoundException(request.DestinationShareId);

        if (sourceShare.Student.Group.InstanceId != destinationShare.Student.Group.InstanceId)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.DestinationShareId),
                "Source and destination share must be in the same instance."
            );
        }

        if (request.WithdrawalLimit)
        {
            _assessWithdrawalLimit(sourceShare, _context);
        }

        if (sourceShare.Balance < request.Amount && !request.TakeNegative)
        {
            throw new NonsufficientFundsException(sourceShare, request.Amount);
        }

        sourceShare.Balance -= request.Amount;
        sourceShare.DateLastActive = DateTime.UtcNow;
        destinationShare.Balance += request.Amount;
        destinationShare.DateLastActive = DateTime.UtcNow;

        string? tranComment = $"Transfer to #{destinationShare.Student.AccountNumber}S{destinationShare.Id}";

        if (request.Comment != null)
        {
            tranComment += $". Comment: {request.Comment}";
        }

        Transaction? sourceTransaction = new Transaction
        {
            Amount = -request.Amount,
            NewBalance = sourceShare.Balance,
            TargetShare = sourceShare,
            TransactionType = "T",
            EffectiveDate = request.EffectiveDate ?? DateTime.UtcNow,
            Comment = tranComment
        };

        tranComment = $"Transfer from #{sourceShare.Student.AccountNumber}";

        if (request.Comment != null)
        {
            tranComment += $". Comment: {request.Comment}";
        }

        Transaction destinationTransaction = new Transaction
        {
            Amount = request.Amount,
            NewBalance = destinationShare.Balance,
            TargetShare = destinationShare,
            TransactionType = "T",
            EffectiveDate = request.EffectiveDate ?? DateTime.UtcNow,
            Comment = tranComment
        };

        _context.Transactions.Add(sourceTransaction);
        _context.Transactions.Add(destinationTransaction);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
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
    public async Task<StudentPurchase> PurchaseAsync(PurchaseRequest input, CancellationToken cancellationToken = new())
    {
        // Validate that the items don't contain negative or zero quantities
        if (input.Items.Any(x => x.Count < 1))
        {
            throw new ArgumentOutOfRangeException(
                nameof(input),
                "Purchase quantities must be greater than zero."
            );
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
                .Select(x => new { x.StudentId, x.Student.Group.InstanceId }).FirstOrDefaultAsync(cancellationToken: cancellationToken)
            ?? throw new ShareNotFoundException(input.ShareId);

        // Pull the list of requested products to validate cost and quantity
        List<long>? productIds = input.Items.Select(x => x.ProductId).ToList();
        List<Product>? products = await _context.ProductInstances
            .Include(x => x.Product)
            .Where(x =>
                x.InstanceId == share.InstanceId
                && productIds.Contains(x.ProductId))
            .Select(x => x.Product)
            .ToListAsync(cancellationToken: cancellationToken);

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
        _context.StudentPurchases.Add(purchase);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            throw new DatabaseException(e.Message);
        }

        try
        {
            var request = new TransactionRequest()
            {
                ShareId = input.ShareId,
                Amount = -total,
                Comment = $"Purchase #{purchase.Id}"
            };

            await PostAsync(request, _context, cancellationToken);
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

                _context.StudentPurchaseItems.Remove(item);
            }

            _context.StudentPurchases.Remove(purchase);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
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
    public async Task<StudentStock> PurchaseStockAsync(PurchaseStockRequest input, CancellationToken cancellationToken = new())
    {
        bool stockExists = await _context
            .Stocks
            .Where(x => x.Id == input.StockId).AnyAsync(cancellationToken);

        if (!stockExists)
        {
            throw new StockNotFoundException(input.StockId);
        }

        // Get the share's Student ID and Instance ID for later use
        var share = await _context
                .Shares
                .Include(x => x.Student)
                .ThenInclude(x => x.Group)
                .ThenInclude(x => x.Instance)
                .Where(x =>
                    x.Id == input.ShareId
                    && x.Student.DateDeleted == null
                    && x.Student.Group.DateDeleted == null
                    && x.Student.Group.Instance.DateDeleted == null)
                .Select(x => new { x.Id, x.StudentId, x.Student.Group.InstanceId })
                .SingleOrDefaultAsync(cancellationToken: cancellationToken)
            ?? throw new ShareNotFoundException(input.ShareId);

        // Get the stock provided it is included in the instance of the share
        Stock? stock = await _context
                .Stocks
                .Include(x => x.StockInstances)
                .Where(x =>
                    x.Id == input.StockId
                    && x.StockInstances.Any(xx => xx.InstanceId == share.InstanceId))
                .SingleOrDefaultAsync(cancellationToken: cancellationToken)
            ?? throw new UnauthorizedPurchaseException();

        // Ensure there are enough shares available to purchase
        if (stock.AvailableShares < input.Quantity)
        {
            throw new InvalidShareQuantityException(stock, input.Quantity);
        }

        // Fetch or create the StudentStock entity
        StudentStock? studentStock = await _context
            .StudentStocks
            .Where(x => x.StudentId == share.StudentId && x.StockId == stock.Id)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (studentStock == null)
        {
            studentStock = new StudentStock
            {
                StudentId = share.StudentId,
                StockId = stock.Id,
                SharesOwned = 0,
                NetContribution = Money.FromCurrency(0m)
            };

            _context.StudentStocks.Add(studentStock);
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
        var request = new TransactionRequest()
        {
            ShareId = share.Id,
            Amount = totalCost,
            Comment = $"Stock {buySell}: {input.Quantity} shares of {stock.Symbol} at {stock.CurrentValue}"
        };

        var transaction = await PostAsync(request, _context, cancellationToken);

        studentStock.History.Add(
        new StudentStockHistory
            {
                Amount = totalCost,
                Count = input.Quantity,
                StudentStock = studentStock,
                Transaction = transaction
            }
        );

        stock.AvailableShares -= input.Quantity;
        studentStock.NetContribution += transaction.Amount * -1;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            try
            {
                request = new TransactionRequest()
                {
                    ShareId = share.Id,
                    Amount = -totalCost,
                    Comment = $"VOIDED: Stock purchase: {input.Quantity} shares of {stock.Symbol}"
                };

                await PostAsync(request, _context, cancellationToken);
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
    public async Task<bool> PostDividendsAsync(PostDividendsRequest input, CancellationToken cancellationToken = new())
    {
        ShareType shareType = await _context
                .ShareTypes
                .Where(x => x.Id == input.ShareTypeId)
                .SingleOrDefaultAsync(cancellationToken: cancellationToken)
            ?? throw new ShareTypeNotFoundException(input.ShareTypeId);

        if (shareType.DividendRate == Rate.FromRate(0.0m))
        {
            throw new ArgumentOutOfRangeException(
                nameof(input),
                "Share Type provided has no dividend rate."
            );
        }

        int instances = await _context
            .Instances
            .Where(x => input.Instances.Contains(x.Id))
            .CountAsync(cancellationToken: cancellationToken);

        if (instances != input.Instances.Count())
        {
            throw new ArgumentOutOfRangeException(
                nameof(input),
                "One or more instances provided do not exist."
            );
        }

        await using IDbContextTransaction? transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        foreach (long instance in input.Instances)
        {
            IQueryable<Share>? query = _context
                .Shares
                .Include(x => x.Student)
                .ThenInclude(x => x.Group)
                .Where(x =>
                    x.ShareTypeId == shareType.Id
                    && x.Student.Group.InstanceId == instance
                    && x.RawBalance > 0);

            int count = await query.CountAsync(cancellationToken: cancellationToken);

            for (int i = 0; i < count; i += 100)
            {
                List<Share>? shares = await query.Skip(i).Take(100).ToListAsync(cancellationToken: cancellationToken);

                foreach (Share? share in shares)
                {
                    Money? dividendAmount = share.Balance * shareType.DividendRate;
                    Money? newBalance = share.Balance += dividendAmount;

                    _context.Transactions.Add(new Transaction
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
                    await _context.SaveChangesAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    throw new DatabaseException(e.Message);
                }
            }
        }

        try
        {
            await transaction.CommitAsync(cancellationToken);
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
    private void _assessWithdrawalLimit(Share share, IAppDbContext context)
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

            context.Transactions.Add(
                new Transaction
                {
                    Amount = shareType.WithdrawalLimitFee,
                    NewBalance = share.Balance,
                    TargetShare = share,
                    TransactionType = "F",
                    EffectiveDate = DateTime.UtcNow,
                    Comment = "Withdrawal Limit Fee"
                }
            );
        }

        share.LimitedWithdrawalCount += 1;
    }
}
