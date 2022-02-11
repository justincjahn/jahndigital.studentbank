using System.Linq;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Application.Common.Utils;
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


    /// <summary>
    ///     Cache the products in the system so we don't have to keep fetching it.
    /// </summary>
    private IReadOnlyCollection<Product> _productsCache = new Product[] { };

    /// <summary>
    ///     Set during seeding if needed.
    /// </summary>
    private ShareType? _shareType;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="factory"></param>
    public DbInitializerService(IDbContextFactory<AppDbContext> factory, IPasswordHasher passwordHasher)
    {
        _factory = factory;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    ///     Perform migrations.
    /// </summary>
    public void Initialize()
    {
        using AppDbContext? context = _factory.CreateDbContext();
        context.Database.Migrate();
    }

    /// <summary>
    ///     Seed the database with core data and optionally random data.
    /// </summary>
    public void SeedData()
    {
        using AppDbContext? context = _factory.CreateDbContext();

        SeedPrivileges(context);
        SeedRoles(context);
        SeedUsers(context);

        #if DEBUG

        // Generates semi-random data for development and testing.
        SeedShareTypes(context);
        SeedProducts(context);
        SeedStocks(context);
        SeedGroups(context, SeedInstances(context));
        #endif
    }

    /// <summary>
    ///     Add privileges to the system if they don't already exist.
    /// </summary>
    private void SeedPrivileges(AppDbContext context)
    {
        List<Privilege>? dbPrivileges = context.Privileges.ToList();

        foreach (PrivilegeEnum privilege in PrivilegeEnum.Privileges)
        {
            if (dbPrivileges.FirstOrDefault(x => x.Name == privilege.Name) == null)
            {
                context.Add(new Privilege { Name = privilege.Name, Description = privilege.Description });
            }
        }

        context.SaveChanges();
    }

    /// <summary>
    ///     Add roles to the system if they don't already exist.
    /// </summary>
    /// <param name="context"></param>
    private void SeedRoles(AppDbContext context)
    {
        List<Privilege>? dbPrivileges = context.Privileges.ToList();
        List<Role>? dbRoles = context.Roles.Where(x => x.IsBuiltIn).ToList();

        foreach (RoleEnum role in RoleEnum.Roles)
        {
            if (dbRoles.FirstOrDefault(x => x.Name == role.Name) == null)
            {
                Role? dbRole = new Role { Name = role.Name, Description = role.Description, IsBuiltIn = true };

                foreach (PrivilegeEnum privilege in role.Privileges)
                {
                    Privilege? priv = dbPrivileges.First(x => x.Name == privilege.Name);
                    dbRole.RolePrivileges.Add(new RolePrivilege { Role = dbRole, Privilege = priv });
                }

                context.Add(dbRole);
            }
        }

        context.SaveChanges();
    }

    /// <summary>
    ///     Insert an admin user in the database if no users exist.
    /// </summary>
    /// <param name="context"></param>
    private void SeedUsers(AppDbContext context)
    {
        Role? superuser = context.Roles.FirstOrDefault(x => x.Name == RoleEnum.Superuser.Name)
            ?? throw new DbUpdateException("Unable to seed admin user- superuser role not found.");

        var users = context.Users.ToList();
        
        if (context.Users.Count() == 0)
        {
            User? admin = new User
            {
                Email = "admin@domain.tld",
                Password = _passwordHasher.HashPassword("admin"),
                Role = superuser,
                DateRegistered = DateTime.UtcNow
            };

            context.Add(admin);
            context.SaveChanges();
        }
    }

    /// <summary>
    ///     Add a savings and checking account type to the database.
    /// </summary>
    /// <param name="context"></param>
    private void SeedShareTypes(AppDbContext context)
    {
        if (!context.ShareTypes.Any())
        {
            _shareType = new ShareType
            {
                DividendRate = Rate.FromRate(0.05m), // 0.05%
                WithdrawalLimitCount = 6,
                WithdrawalLimitPeriod = Period.Weekly,
                Name = "Savings"
            };

            context.Add(_shareType);
            context.Add(new ShareType { Name = "Checking", DividendRate = Rate.FromRate(0) });

            context.SaveChanges();
        }
        else
        {
            _shareType = context.ShareTypes.FirstOrDefault(x => x.RawDividendRate > 0);

            if (_shareType == null)
            {
                _shareType = context.ShareTypes.FirstOrDefault();
            }
        }
    }

    /// <summary>
    ///     Add some fake products to the database as examples.
    /// </summary>
    /// <param name="context"></param>
    private void SeedProducts(AppDbContext context)
    {
        if (context.Products.Any())
        {
            return;
        }

        context.Add(new Product
        {
            Name = "Extra Credit",
            Description = "Redeem your balance for 5 points of extra credit.",
            Cost = Money.FromCurrency(10.00m),
            IsLimitedQuantity = false
        });

        context.Add(new Product
        {
            Name = "Chocolate Bar",
            Description = "Redeem your balance for a Harsley's chocolate bar.",
            Cost = Money.FromCurrency(3.50m),
            IsLimitedQuantity = true,
            Quantity = 128
        });

        context.SaveChanges();
    }

    /// <summary>
    ///     Add some fake stocks to the database.
    /// </summary>
    /// <param name="context"></param>
    private void SeedStocks(AppDbContext context)
    {
        if (context.Stocks.Any())
        {
            return;
        }

        List<Stock>? stocks = new List<Stock>();

        for (int i = 0; i < 5; i++)
        {
            int amount = new Random().Next(30, 250);
            Stock? stock = new Stock
            {
                Name = $"Stock {i + 1}",
                Symbol = $"STK{i + 1}",
                TotalShares = 10000000,
                AvailableShares = 10000000,
                CurrentValue = Money.FromCurrency(amount)
            };

            stocks.Add(stock);
            context.Add(stock);
        }

        context.SaveChanges();
        SeedStockHistory(context, stocks);
    }

    /// <summary>
    ///     Generate random stock history for a random number of days.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="stocks"></param>
    private void SeedStockHistory(AppDbContext context, IEnumerable<Stock> stocks)
    {
        foreach (Stock? stock in stocks)
        {
            int days = new Random().Next(5, 45);

            for (int i = days; i >= 0; i--)
            {
                decimal percent = new Random().Next(1, 80);
                percent *= new Random().Next(0, 2) == 1 ? 1 : -1;
                percent = (percent / 100) + 1;

                Money? newAmount = stock.CurrentValue * Rate.FromRate(percent);
                newAmount = newAmount.Amount > 0 ? newAmount : Money.FromCurrency(0.1M);

                context.Add(new StockHistory
                {
                    DateChanged = DateTime.UtcNow.AddDays(i * -1), Stock = stock, Value = newAmount
                });

                stock.CurrentValue = newAmount;
            }
        }

        context.SaveChanges();
    }

    /// <summary>
    ///     Seed instances.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private IEnumerable<Instance> SeedInstances(AppDbContext context)
    {
        List<ShareType>? shareTypes = context.ShareTypes.ToList();
        List<Product>? products = context.Products.ToList();
        List<Stock>? stocks = context.Stocks.ToList();
        List<Instance>? instances = new List<Instance>();

        if (!context.Instances.Any())
        {
            for (int i = 0; i < 3; i++)
            {
                // Generate a unique invite code
                string code;

                do
                {
                    code = InviteCode.NewCode(); // Using default length for seeding
                } while (instances.Any(x => x.InviteCode == code));

                Instance? instance = new Instance { Description = $"Instance {i}", InviteCode = code };

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

            context.SaveChanges();
        }

        return instances;
    }

    /// <summary>
    ///     Seed groups into the provided instances and link them to all products found in the database.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="instances"></param>
    private void SeedGroups(AppDbContext context, IEnumerable<Instance> instances)
    {
        List<Product>? products = context.Products.ToList();

        foreach (Instance? instance in instances)
        {
            List<Group>? groups = new List<Group>();

            for (int i = 0; i < 5; i++)
            {
                Group? group = new Group { Name = $"Group {i}", Instance = instance };

                groups.Add(group);
                context.Groups.Add(group);
            }

            context.SaveChanges();
            SeedStudents(context, groups);
        }

        // After everything is created and transactions are posted, purchase stuff
        if (instances.Count() > 0)
        {
            SeedPurchases(context);
        }
    }

    /// <summary>
    ///     Seed students into the provided groups.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="groups"></param>
    private void SeedStudents(AppDbContext context, IEnumerable<Group> groups)
    {
        foreach (Group? group in groups)
        {
            List<Student>? students = new List<Student>();
            int max = new Random().Next(5, 100);

            for (int i = 0; i <= max; i++)
            {
                string? accountNumber = $"{group.Id}{i}".PadLeft(10, '0');
                Student? student = new Student
                {
                    AccountNumber = accountNumber,
                    Group = group,
                    Password = _passwordHasher.HashPassword("student"),
                    Email = $"student{i}@group{group.Id}.domain.tld",
                    FirstName = "Student",
                    LastName = $"{i}",
                    DateRegistered = DateTime.UtcNow
                };

                context.Students.Add(student);
                students.Add(student);
            }

            context.SaveChanges();
            SeedShares(context, students);
        }
    }

    /// <summary>
    ///     Creates initial shares for the students provided.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="students"></param>
    private void SeedShares(AppDbContext context, IEnumerable<Student> students)
    {
        List<Share>? shares = new List<Share>();

        foreach (Student? student in students)
        {
            Share? share = new Share
            {
                Student = student,
                Balance = Money.FromCurrency(0),
                ShareType = _shareType!,
                DateLastActive = DateTime.UtcNow
            };

            shares.Add(share);
            context.Add(share);
        }

        context.SaveChanges();
        SeedTransactions(context, shares);
    }

    /// <summary>
    ///     Seed a random number of transactions for the provided students.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="shares"></param>
    private void SeedTransactions(AppDbContext context, IEnumerable<Share> shares)
    {
        foreach (Share? share in shares)
        {
            int max = new Random().Next(5, 500);

            for (int i = 0; i <= max; i++)
            {
                int amount = new Random().Next(1, 50000); // between $0.01 and $500.00
                amount *= new Random().Next(0, 2) == 1 ? 1 : -1;
                Money? oAmount = Money.FromDatabase(amount);
                Money? newBalance = share.Balance + oAmount;

                Transaction? transaction = new Transaction
                {
                    EffectiveDate = DateTime.UtcNow,
                    Amount = oAmount,
                    TargetShare = share,
                    NewBalance = newBalance,
                    TransactionType = oAmount.Amount > 0.00m ? "D" : "W"
                };

                Console.WriteLine($"Share {share.Id}: ${oAmount.Amount}");

                share.Balance = newBalance;
                context.Add(transaction);
            }
        }

        context.SaveChanges();
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
    private void SeedPurchases(AppDbContext context)
    {
        _productsCache = context.Products.ToList();

        if (_productsCache.Count == 0)
        {
            return;
        }

        // Loop through all student shares with a positive balance and build a purchase
        foreach (Share? share in context.Shares.Where(x => x.RawBalance >= 0))
        {
            StudentPurchase? purchase = new StudentPurchase { StudentId = share.StudentId };

            SeedPurchasesExtraCredit(context, share, purchase);
            SeedPurchasesLimited(context, share, purchase);

            // Generate totals and transaction
            Money? totalCost = Money.FromCurrency(0);
            purchase.Items.ToList().ForEach(x => totalCost = totalCost + x.PurchasePrice);
            purchase.TotalCost = totalCost;

            context.Add(new Transaction
            {
                Amount = totalCost,
                EffectiveDate = DateTime.UtcNow,
                NewBalance = share.Balance,
                TargetShare = share,
                TransactionType = "W"
            });

            context.Update(share);
            context.Add(purchase);
        }

        context.SaveChanges();
    }

    /// <summary>
    ///     Add some extra credit to the purchase if there's enough money.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="share"></param>
    /// <param name="purchase"></param>
    private void SeedPurchasesExtraCredit(AppDbContext context, Share share, StudentPurchase purchase)
    {
        Product? extraCredit = _productsCache.FirstOrDefault(x => x.IsLimitedQuantity = true);

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
        StudentPurchaseItem? item = new StudentPurchaseItem
        {
            Product = extraCredit,
            PurchasePrice = extraCredit.Cost * quantity,
            StudentPurchase = purchase,
            Quantity = quantity
        };

        purchase.Items.Add(item);
        share.Balance = share.Balance - item.PurchasePrice;
        context.Add(item);

        Console.WriteLine($"Share {share.Id}: Extra Credit ({quantity}) - {share.Balance}");
    }

    /// <summary>
    ///     Add some limited quantity items to shares until there are none left.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="share"></param>
    /// <param name="purchase"></param>
    private void SeedPurchasesLimited(AppDbContext context, Share share, StudentPurchase purchase)
    {
        Product? chocolateBar = _productsCache.FirstOrDefault(x => x.IsLimitedQuantity);

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
        StudentPurchaseItem? item = new StudentPurchaseItem
        {
            Product = chocolateBar, PurchasePrice = chocolateBar.Cost, StudentPurchase = purchase, Quantity = 1
        };

        chocolateBar.Quantity--;
        context.Update(chocolateBar);

        purchase.Items.Add(item);
        share.Balance = share.Balance - item.PurchasePrice;
        context.Add(item);

        Console.WriteLine($"Share {share.Id}: Chocolate (1) - {share.Balance}");
    }
}
