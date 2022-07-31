using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Privilege = JahnDigital.StudentBank.Domain.Entities.Privilege;
using PrivilegeEnum = JahnDigital.StudentBank.Domain.Enums.Privilege;
using Role = JahnDigital.StudentBank.Domain.Entities.Role;
using RoleEnum = JahnDigital.StudentBank.Domain.Enums.Role;

namespace JahnDigital.StudentBank.Infrastructure.Persistence;

/// <summary>
///     Handles automatic database migrations and seeding.
/// </summary>
public class DbInitializerService : IDbInitializerService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IInviteCodeGenerator _inviteCodeGenerator;

    /// <summary>
    ///     Cache the products in the system so we don't have to keep fetching it.
    /// </summary>
    private IReadOnlyCollection<Product> _productsCache = Array.Empty<Product>();

    /// <summary>
    ///
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="passwordHasher"></param>
    /// <param name="inviteCodeGenerator"></param>
    public DbInitializerService(
        IDbContextFactory<AppDbContext> factory,
        IPasswordHasher passwordHasher,
        IInviteCodeGenerator inviteCodeGenerator
    )
    {
        _factory = factory;
        _passwordHasher = passwordHasher;
        _inviteCodeGenerator = inviteCodeGenerator;
    }

    /// <summary>
    ///     Perform migrations.
    /// </summary>
    public async Task InitializeAsync()
    {
        await using AppDbContext context = await _factory.CreateDbContextAsync();
        await context.Database.MigrateAsync();
    }

    /// <summary>
    ///     Seed the database with core data and optionally random data.
    /// </summary>
    public async Task SeedDataAsync()
    {
        await using AppDbContext context = await _factory.CreateDbContextAsync();

        await SeedPrivileges(context);
        await SeedRoles(context);
        await SeedUsers(context);

        // Generates semi-random data for development and testing.
        #if !DEBUG
            return;
        #endif

        await SeedShareTypes(context);
        await SeedProducts(context);
        await SeedStocks(context);
        var groups = await SeedGroups(context, await SeedInstances(context));
        var students = await SeedStudents(context, groups);
        var shares = (await SeedShares(context, students)).ToArray();
        await SeedTransactions(context, shares);
        await SeedPurchases(context, shares);
    }

    /// <summary>
    ///     Add privileges to the system if they don't already exist.
    /// </summary>
    private static async Task SeedPrivileges(IAppDbContext context)
    {
        List<Privilege> dbPrivileges = context.Privileges.ToList();

        foreach (PrivilegeEnum privilege in PrivilegeEnum.Privileges)
        {
            if (dbPrivileges.FirstOrDefault(x => x.Name == privilege.Name) == null)
            {
                context.Privileges.Add(new Privilege
                {
                    Name = privilege.Name,
                    Description = privilege.Description
                });
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Add roles to the system if they don't already exist.
    /// </summary>
    /// <param name="context"></param>
    private static async Task SeedRoles(IAppDbContext context)
    {
        List<Privilege> dbPrivileges = context.Privileges.ToList();
        List<Role> dbRoles = context.Roles.Where(x => x.IsBuiltIn).ToList();

        foreach (RoleEnum role in RoleEnum.Roles)
        {
            if (dbRoles.FirstOrDefault(x => x.Name == role.Name) == null)
            {
                Role dbRole = new() { Name = role.Name, Description = role.Description, IsBuiltIn = true };

                foreach (PrivilegeEnum privilege in role.Privileges)
                {
                    Privilege priv = dbPrivileges.First(x => x.Name == privilege.Name);
                    dbRole.RolePrivileges.Add(new RolePrivilege { Role = dbRole, Privilege = priv });
                }

                context.Roles.Add(dbRole);
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Insert an admin user in the database if no users exist.
    /// </summary>
    /// <param name="context"></param>
    private async Task SeedUsers(IAppDbContext context)
    {
        Role superuser = context.Roles.FirstOrDefault(x => x.Name == RoleEnum.Superuser.Name)
            ?? throw new DbUpdateException("Unable to seed admin user superuser role not found.  There was an error seeding roles.");

        if (!context.Users.Any())
        {
            User admin = new()
            {
                Email = "admin@domain.tld",
                Password = _passwordHasher.HashPassword("admin"),
                Role = superuser,
                DateRegistered = DateTime.UtcNow
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    ///     Add a savings and checking account type to the database.
    /// </summary>
    /// <param name="context"></param>
    private static async Task SeedShareTypes(IAppDbContext context)
    {
        if (context.ShareTypes.Any())
        {
            return;
        }

        context.ShareTypes.Add(new ShareType
        {
            Name = "Savings",
            DividendRate = Rate.FromRate(0.05m), // 0.05%
            WithdrawalLimitCount = 6,
            WithdrawalLimitPeriod = Period.Weekly
        });

        context.ShareTypes.Add(new ShareType
        {
            Name = "Checking",
            DividendRate = Rate.FromRate(0)
        });

        await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Add some fake products to the database as examples.
    /// </summary>
    /// <param name="context"></param>
    private static async Task SeedProducts(IAppDbContext context)
    {
        if (context.Products.Any())
        {
            return;
        }

        context.Products.Add(new Product
        {
            Name = "Extra Credit",
            Description = "Redeem your balance for 5 points of extra credit.",
            Cost = Money.FromCurrency(10.00m),
            IsLimitedQuantity = false
        });

        context.Products.Add(new Product
        {
            Name = "Chocolate Bar",
            Description = "Redeem your balance for a Harsley's chocolate bar.",
            Cost = Money.FromCurrency(3.50m),
            IsLimitedQuantity = true,
            Quantity = 128
        });

        await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Add some fake stocks to the database.
    /// </summary>
    /// <param name="context"></param>
    private static async Task SeedStocks(IAppDbContext context)
    {
        if (context.Stocks.Any())
        {
            return;
        }

        List<Stock> stocks = new();

        for (int i = 0; i < 5; i++)
        {
            int amount = new Random().Next(30, 250);

            Stock stock = new()
            {
                Name = $"Stock {i + 1}",
                Symbol = $"STK{i + 1}",
                TotalShares = 10000000,
                AvailableShares = 10000000,
                CurrentValue = Money.FromCurrency(amount)
            };

            stocks.Add(stock);
            context.Stocks.Add(stock);
        }

        await context.SaveChangesAsync();
        await SeedStockHistory(context, stocks);
    }

    /// <summary>
    ///     Generate random stock history for a random number of days.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="stocks"></param>
    private static async Task SeedStockHistory(IAppDbContext context, IEnumerable<Stock> stocks)
    {
        foreach (var stock in stocks)
        {
            int days = new Random().Next(5, 45);

            Money lastValue = stock.CurrentValue;
            for (int i = days; i >= 0; i--)
            {
                decimal percent = new Random().Next(1, 80);
                percent *= new Random().Next(0, 2) == 1 ? 1 : -1;
                percent = (percent / 100) + 1;

                Money newAmount = lastValue * Rate.FromRate(percent);
                newAmount = newAmount.Amount > 0 ? newAmount : Money.FromCurrency(0.1M);

                // Since we're simulating updates at specific days, bypass the domain logic
                if (i != 0) {
                    context.StockHistory.Add(new StockHistory
                    {
                        Stock = stock,
                        Value = newAmount,
                        DateChanged = DateTime.UtcNow.AddDays(i * -1),
                    });
                }

                lastValue = newAmount;
            }

            // Allow the domain logic to create the final StockHistory object
            stock.CurrentValue = lastValue;
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Seed instances.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<IEnumerable<Instance>> SeedInstances(IAppDbContext context)
    {
        List<ShareType> shareTypes = context.ShareTypes.ToList();
        List<Product> products = context.Products.ToList();
        List<Stock> stocks = context.Stocks.ToList();
        List<Instance> instances = new();

        if (!context.Instances.Any())
        {
            for (int i = 0; i < 3; i++)
            {
                // Generate a unique invite code
                string code;

                do
                {
                    code = _inviteCodeGenerator.NewCode(6); // Using default length for seeding
                } while (instances.Any(x => x.InviteCode == code));

                Instance instance = new() { Description = $"Instance {i}", InviteCode = code };

                shareTypes.ForEach(x => instance.ShareTypeInstances.Add(
                    new ShareTypeInstance { Instance = instance, ShareType = x }
                ));

                products.ForEach(x => instance.ProductInstances.Add(
                    new ProductInstance { Instance = instance, Product = x }
                ));

                stocks.ForEach(x => instance.StockInstances.Add(
                    new StockInstance { Instance = instance, Stock = x }
                ));

                instances.Add(instance);
                context.Instances.Add(instance);
            }

            await context.SaveChangesAsync();
        }

        return instances;
    }

    /// <summary>
    ///     Seed groups into the provided instances and link them to all products found in the database.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="instances"></param>
    private static async Task<IEnumerable<Group>> SeedGroups(IAppDbContext context, IEnumerable<Instance> instances)
    {
        List<Group> groups = new();
        foreach (Instance instance in instances)
        {
            for (int i = 0; i < 5; i++)
            {
                Group group = new() { Name = $"Group {i}", Instance = instance };

                groups.Add(group);
                context.Groups.Add(group);
            }
        }

        await context.SaveChangesAsync();
        return groups;
    }

    /// <summary>
    ///     Seed students into the provided groups.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="groups"></param>
    private async Task<IEnumerable<Student>> SeedStudents(IAppDbContext context, IEnumerable<Group> groups)
    {
        List<Student> students = new();

        foreach (Group group in groups)
        {
            int max = new Random().Next(5, 100);

            for (int i = 0; i <= max; i++)
            {
                string accountNumber = $"{group.Id}{i}".PadLeft(10, '0');
                Student student = new()
                {
                    AccountNumber = accountNumber,
                    Group = group,
                    Password = _passwordHasher.HashPassword("student"),
                    Email = $"student{i}@group{group.Id}.domain.tld",
                    FirstName = "Student",
                    LastName = $"{i}",
                    DateRegistered = DateTime.UtcNow
                };

                students.Add(student);
                context.Students.Add(student);
            }
        }

        await context.SaveChangesAsync();
        return students;
    }

    /// <summary>
    ///     Creates initial shares for the students provided.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="students"></param>
    private static async Task<IEnumerable<Share>> SeedShares(IAppDbContext context, IEnumerable<Student> students)
    {
        var shareType = context.ShareTypes.FirstOrDefault(x => x.RawDividendRate > 0)
            ?? context.ShareTypes.FirstOrDefault()
            ?? throw new NotFoundException("No ShareType records found in the database.  There was an error seeding Share Types.");

        List<Share> shares = new();
        foreach (Student student in students)
        {
            Share share = new()
            {
                Student = student,
                Balance = Money.FromCurrency(0),
                ShareType = shareType,
                DateLastActive = DateTime.UtcNow
            };

            shares.Add(share);
            context.Shares.Add(share);
        }

        await context.SaveChangesAsync();
        return shares;
    }

    /// <summary>
    ///     Seed a random number of transactions for the provided students.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="shares"></param>
    private static async Task SeedTransactions(IAppDbContext context, IEnumerable<Share> shares)
    {
        foreach (Share share in shares)
        {
            int max = new Random().Next(5, 500);

            for (int i = 0; i <= max; i++)
            {
                int amount = new Random().Next(1, 50000); // between $0.01 and $500.00
                amount *= new Random().Next(0, 2) == 1 ? 1 : -1;
                Money oAmount = Money.FromDatabase(amount);
                Money newBalance = share.Balance + oAmount;

                Transaction transaction = new()
                {
                    EffectiveDate = DateTime.UtcNow,
                    Amount = oAmount,
                    TargetShare = share,
                    NewBalance = newBalance,
                    TransactionType = oAmount.Amount > 0.00m ? "D" : "W"
                };

                share.Balance = newBalance;
                context.Transactions.Add(transaction);
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Select some students with a positive balance and purchase some stuff.
    /// </summary>
    /// <remarks>
    ///     In the application API, we should be checking to ensure the item being purchased
    ///     was linked to the group of the student's share, but everything is linked to
    ///     everything when we seed initially.
    /// </remarks>
    /// <param name="context"></param>
    /// <param name="shares"></param>
    private async Task SeedPurchases(IAppDbContext context, IEnumerable<Share> shares)
    {
        _productsCache = context.Products.ToList();

        if (_productsCache.Count == 0)
        {
            return;
        }

        // Loop through all student shares with a positive balance and build a purchase
        foreach (Share share in shares.Where(x => x.RawBalance >= 0))
        {
            StudentPurchase purchase = new() { StudentId = share.StudentId };

            SeedPurchasesExtraCredit(context, share, purchase);
            SeedPurchasesLimited(context, share, purchase);

            // Generate totals and transaction
            Money totalCost = Money.Zero;
            purchase.Items.ToList().ForEach(x => totalCost += x.PurchasePrice);
            purchase.TotalCost = totalCost;

            context.Transactions.Add(new Transaction
            {
                Amount = totalCost,
                EffectiveDate = DateTime.UtcNow,
                NewBalance = share.Balance,
                TargetShare = share,
                TransactionType = "W"
            });

            context.StudentPurchases.Add(purchase);
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Add some extra credit to the purchase if there's enough money.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="share"></param>
    /// <param name="purchase"></param>
    private void SeedPurchasesExtraCredit(IAppDbContext context, Share share, StudentPurchase purchase)
    {
        Product? extraCredit = _productsCache.FirstOrDefault(x => x.IsLimitedQuantity = false);

        if (extraCredit == null)
        {
            return;
        }

        // Get the max number of extra credits we can purchase with the balance we have
        int max = decimal.ToInt32(Math.Floor(decimal.Divide(share.Balance.Amount, extraCredit.Cost.Amount)));

        // Get a random number as the quantity for the purchase.
        int quantity = new Random().Next(0, max);

        if (quantity < 1)
        {
            return;
        }

        // Add the purchase, subtract from share balance
        StudentPurchaseItem item = new()
        {
            Product = extraCredit,
            StudentPurchase = purchase,
            Quantity = quantity
        };

        purchase.AddPurchaseItem(item);
        share.Balance -= item.PurchasePrice;
        context.StudentPurchaseItems.Add(item);
    }

    /// <summary>
    ///     Add some limited quantity items to shares until there are none left.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="share"></param>
    /// <param name="purchase"></param>
    private void SeedPurchasesLimited(IAppDbContext context, Share share, StudentPurchase purchase)
    {
        Product? chocolateBar = _productsCache.FirstOrDefault(x => x.IsLimitedQuantity = true);

        if (chocolateBar == null)
        {
            return;
        }

        if (chocolateBar.Quantity < 0)
        {
            return;
        }

        // Make sure the share can afford a chocolate bar
        if (chocolateBar.Cost > share.Balance)
        {
            return;
        }

        // Randomly decide if this student is buying a chocolate bar
        if (new Random().Next(0, 6) != 1)
        {
            return; // 1 in 6
        }

        // Add the purchase, subtract from share balance
        StudentPurchaseItem item = new()
        {
            Product = chocolateBar,
            StudentPurchase = purchase,
            Quantity = 1
        };

        chocolateBar.Quantity--;
        purchase.AddPurchaseItem(item);
        share.Balance -= item.PurchasePrice;
        context.StudentPurchaseItems.Add(item);
    }
}
