using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using jahndigital.studentbank.utils;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.dal.Contexts;

namespace jahndigital.studentbank.server.Services
{
    /// <summary>
    /// Handles automatic database migrations and seeding.
    /// </summary>
    public class DbInitializerService : IDbInitializerService
    {
        /// <summary>
        /// Set during seeding if needed.
        /// </summary>
        private ShareType? _shareType;

        /// <summary>
        /// Cache the products in the system so we don't have to keep fetching it.
        /// </summary>
        private IReadOnlyCollection<Product> _productsCache = new Product[] {};

        /// <summary>
        /// 
        /// </summary>
        private readonly IServiceScopeFactory _scopeFactory;

        /// <summary>
        /// Pull in the services from dependency injection api.
        /// </summary>
        /// <param name="scopeFactory"></param>
        public DbInitializerService(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

        /// <summary>
        /// Perform migrations.
        /// </summary>
        public void Initialize()
        {
            using var serviceScope = _scopeFactory.CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<AppDbContext>();
            if (context == null) throw new ArgumentNullException("Expected an AppDbContext.");
            context.Database.Migrate();
        }

        /// <summary>
        /// Seed the database with core data and optionally random data.
        /// </summary>
        public void SeedData()
        {
            using var serviceScope = _scopeFactory.CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<AppDbContext>();

            if (context == null) {
                throw new ArgumentNullException("Expected an AppDbContext.");
            }

            SeedPrivileges(context);
            SeedRoles(context);
            SeedUsers(context);

            #if DEBUG
                // Generates semi-random data for development and testing.
                SeedShareTypes(context);
                SeedProducts(context);
                SeedGroups(context, SeedInstances(context));
            #endif
        }

        /// <summary>
        /// Add privileges to the system if they don't already exist.
        /// </summary>
        public void SeedPrivileges(AppDbContext context)
        {
            List<Privilege> dbPrivileges = context.Privileges.ToList();

            foreach (var privilege in Constants.Privilege.Privileges) {
                if (dbPrivileges.Where(x => x.Name == privilege.Name).FirstOrDefault() == null) {
                    context.Add(new Privilege {
                        Name = privilege.Name,
                        Description = privilege.Description
                    });
                }
            }

            context.SaveChanges();
        }

        /// <summary>
        /// Add roles to the system if they don't already exist.
        /// </summary>
        /// <param name="context"></param>
        public void SeedRoles(AppDbContext context)
        {
            List<Privilege> dbPrivileges = context.Privileges.ToList();
            List<Role> dbRoles = context.Roles.Where(x => x.IsBuiltIn == true).ToList();

            foreach (var role in Constants.Role.Roles) {
                if (dbRoles.Where(x => x.Name == role.Name).FirstOrDefault() == null) {
                    var dbRole = new Role {
                        Name = role.Name,
                        Description = role.Description,
                        IsBuiltIn = true,
                    };

                    foreach (var privilege in role.Privileges) {
                        var priv = dbPrivileges.Where(x => x.Name == privilege.Name).First();
                        dbRole.RolePrivileges.Add(new RolePrivilege {
                            Role = dbRole,
                            Privilege = priv
                        });
                    }

                    context.Add(dbRole);
                }
            }

            context.SaveChanges();
        }

        /// <summary>
        /// Insert an admin user in the database if no users exist.
        /// </summary>
        /// <param name="context"></param>
        public void SeedUsers(AppDbContext context)
        {
            Role superuser = context.Roles.Where(x => x.Name == Constants.Role.Superuser.Name).FirstOrDefault()
                ?? throw new DbUpdateException("Unable to seed admin user- superuser role not found.");

            if (!context.Users.Any()) {
                var admin = new User {
                    Email = "admin@domain.tld",
                    Password = "admin",
                    Role = superuser,
                    DateRegistered = DateTime.UtcNow
                };

                context.Add(admin);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Add a savings and checking account type to the database.
        /// </summary>
        /// <param name="context"></param>
        public void SeedShareTypes(AppDbContext context)
        {
            if (!context.ShareTypes.Any()) {
                _shareType = new ShareType {
                    DividendRate = Rate.FromRate(0.05m), // 0.05%
                    Name = "Savings"
                };

                context.Add(_shareType);
                context.Add(new ShareType {
                    Name = "Checking",
                    DividendRate = Rate.FromRate(0)
                });

                context.SaveChanges();
            } else {
                _shareType = context.ShareTypes.Where(x => x.RawDividendRate > 0).FirstOrDefault();
                if (_shareType == null) {
                    _shareType = context.ShareTypes.FirstOrDefault();
                }
            }
        }

        /// <summary>
        /// Add some fake products to the database as examples.
        /// </summary>
        /// <param name="context"></param>
        public void SeedProducts(AppDbContext context)
        {
            if (context.Products.Any()) return;

            context.Add(new Product {
                Name = "Extra Credit",
                Description = "Redeem your balance for 5 points of extra credit.",
                Cost = Money.FromCurrency(10.00m),
                IsLimitedQuantity = false
            });

            context.Add(new Product {
                Name = "Chocolate Bar",
                Description = "Redeem your balance for a Harsley's chocolate bar.",
                Cost = Money.FromCurrency(3.50m),
                IsLimitedQuantity = true,
                Quantity = 128
            });

            context.SaveChangesAsync();
        }

        /// <summary>
        /// Seed instances.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private IEnumerable<Instance> SeedInstances(AppDbContext context)
        {
            var shareTypes = context.ShareTypes.ToList();
            var products = context.Products.ToList();
            var instances = new List<Instance>();

            if (!context.Instances.Any()) {
                for (var i = 0; i < 3; i++) {
                    // Generate a unique invite code
                    string code;

                    do {
                        code = InviteCode.NewCode(); // Using default length for seeding
                    } while (instances.Any(x => x.InviteCode == code));

                    var instance = new Instance {
                        Description = $"Instance {i}",
                        InviteCode = code
                    };

                    shareTypes.ForEach(x => instance.ShareTypeInstances.Add(
                        new ShareTypeInstance {
                            Instance = instance,
                            ShareType = x
                        }
                    ));

                    products.ForEach(x => instance.ProductInstances.Add(
                        new ProductInstance {
                            Instance = instance,
                            Product = x
                        }
                    ));

                    instances.Add(instance);
                    context.Instances.Add(instance);
                }

                context.SaveChanges();
            }

            return instances;
        }

        /// <summary>
        /// Seed groups into the provided instances and link them to all products found in the database.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instances"></param>
        private void SeedGroups(AppDbContext context, IEnumerable<Instance> instances)
        {
            var products = context.Products.ToList();
            foreach (var instance in instances) {
                var groups = new List<Group>();
                for (var i = 0; i < 5; i++) {
                    var group = new Group
                    {
                        Name = $"Group {i}",
                        Instance = instance
                    };

                    groups.Add(group);
                    context.Groups.Add(group);
                }

                context.SaveChanges();
                SeedStudents(context, groups);
            }

            // After everything is created and transactions are posted, purchase stuff
            if (instances.Count() > 0) {
                SeedPurchases(context);
            }
        }

        /// <summary>
        /// Seed students into the provided groups.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="groups"></param>
        private void SeedStudents(AppDbContext context, IEnumerable<Group> groups)
        {
            foreach (var group in groups) {
                var students = new List<Student>();
                var max = new Random().Next(5, 100);
                for (var i = 0; i <= max; i++) {
                    var accountNumber = $"{group.Id}{i}".PadLeft(10, '0');
                    var student = new Student {
                        AccountNumber = accountNumber,
                        Group = group,
                        Password = "student",
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
        /// Creates initial shares for the students provided.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="students"></param>
        private void SeedShares(AppDbContext context, IEnumerable<Student> students)
        {
            var shares = new List<Share>();
            foreach (var student in students) {
                var share = new Share {
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
        /// Seed a random number of transactions for the provided students.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="shares"></param>
        private void SeedTransactions(AppDbContext context, IEnumerable<Share> shares)
        {
            foreach (var share in shares) {
                var max = new Random().Next(5, 500);
                for (var i = 0; i <= max; i++) {
                    var amount = new Random().Next(1, 50000); // between $0.01 and $500.00
                    amount *= new Random().Next(0, 2) == 1 ? 1 : -1;
                    var oAmount = Money.FromDatabase(amount);
                    var newBalance = share.Balance + oAmount;

                    var transaction = new Transaction {
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
        /// Select some students with a positive balance and purchase some stuff.
        /// </summary>
        /// <remarks>
        /// In the application API, we should be checking to ensure the item being purchased
        /// was linked to the group of the student's share, but everything is linked to
        /// everything when we seed initially.
        /// </remarks>
        /// <param name="context"></param>
        private void SeedPurchases(AppDbContext context)
        {
            _productsCache = context.Products.ToList();
            if (_productsCache.Count == 0) return;

            // Loop through all student shares with a positive balance and build a purchase
            foreach (var share in context.Shares.Where(x => x.RawBalance >= 0)) {
                var purchase = new StudentPurchase {
                    StudentId = share.StudentId
                };

                SeedPurchasesExtraCredit(context, share, purchase);
                SeedPurchasesLimited(context, share, purchase);

                // Generate totals and transaction
                var totalCost = Money.FromCurrency(0);
                purchase.Items.ToList().ForEach(x => totalCost = totalCost + x.PurchasePrice);
                purchase.TotalCost = totalCost;

                context.Add(new Transaction {
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
        /// Add some extra credit to the purchase if there's enough money.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="share"></param>
        /// <param name="purchase"></param>
        private void SeedPurchasesExtraCredit(AppDbContext context, Share share, StudentPurchase purchase)
        {
            var extraCredit = _productsCache.Where(x => x.IsLimitedQuantity = true).FirstOrDefault();
            if (extraCredit == null) return;

            // Get the max number of extra credits we can purchase with the balance we have
            var max = decimal.ToInt32(Math.Floor(decimal.Divide(share.Balance.Amount, extraCredit.Cost.Amount)));

            // Get a random number as the quantity for the purchase.
            var quantity = new Random().Next(0, max);
            if (quantity < 1) return;

            // Add the purchase, subtract from share balance
            var item = new StudentPurchaseItem {
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
        /// Add some limited quantity items to shares until there are none left.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="share"></param>
        /// <param name="purchase"></param>
        private void SeedPurchasesLimited(AppDbContext context, Share share, StudentPurchase purchase)
        {
            var chocolateBar = _productsCache.Where(x => x.IsLimitedQuantity == true).FirstOrDefault();
            if (chocolateBar == null) return;
            if (chocolateBar.Quantity < 0) return;

            // Make sure the share can afford a chocolate bar
            if (chocolateBar.Cost > share.Balance) return;

            // Randomly decide if this student is buying a chocolate bar
            if (new Random().Next(0, 6) != 1) return; // 1 in 6

            // Add the purchase, subtract from share balance
            var item = new StudentPurchaseItem {
                Product = chocolateBar,
                PurchasePrice = chocolateBar.Cost,
                StudentPurchase = purchase,
                Quantity = 1
            };

            chocolateBar.Quantity--;
            context.Update(chocolateBar);

            purchase.Items.Add(item);
            share.Balance = share.Balance - item.PurchasePrice;
            context.Add(item);

            Console.WriteLine($"Share {share.Id}: Chocolate (1) - {share.Balance}");
        }
    }
}
