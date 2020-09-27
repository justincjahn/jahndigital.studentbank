using System;
using System.Collections.Generic;
using System.Linq;
using jahndigital.studentbank.server.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
        private ShareType _shareType;

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
            context.Database.Migrate();
        }

        /// <summary>
        /// Seed the database with core data and optionally random data.
        /// </summary>
        public void SeedData()
        {
            using var serviceScope = _scopeFactory.CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<AppDbContext>();

            SeedPrivileges(context);
            SeedRoles(context);
            SeedUsers(context);
            SeedShareTypes(context);

            #if DEBUG
                // Generates semi-random data for development and testing.
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
            Privilege all = context.Privileges.Where(x => x.Name == Constants.Privilege.All.Name).FirstOrDefault();
            Role superuser = context.Roles.Where(x => x.Name == Constants.Role.Superuser.Name).FirstOrDefault();

            if (!context.Users.Any()) {
                var admin = new User {
                    Email = "admin@domain.tld",
                    Password = "admin",
                    Role = superuser
                };

                context.Add(admin);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void SeedShareTypes(AppDbContext context)
        {
            if (!context.ShareTypes.Any())
            {
                _shareType = new ShareType
                {
                    DividendRate = Rate.FromRate(500), // 0.05%
                    Name = "Savings"
                };

                context.Add(_shareType);
                context.Add(new ShareType
                {
                    Name = "Checking",
                    DividendRate = Rate.FromRate(0)
                });

                context.SaveChanges();
            } else {
                _shareType = context.ShareTypes.Where(x => x.RawDividendRate > 0).FirstOrDefault();
                if (_shareType == null)
                {
                    _shareType = context.ShareTypes.FirstOrDefault();
                }
            }
        }

        /// <summary>
        /// Seed instances.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private IEnumerable<Instance> SeedInstances(AppDbContext context)
        {
            var instances = new List<Instance>();
            if (!context.Instances.Any())
            {
                for (var i = 0; i < 3; i++)
                {
                    var instance = new Instance { Description = $"Instance {i}" };
                    instances.Add(instance);
                    context.Instances.Add(instance);
                }

                context.SaveChanges();
            }

            return instances;
        }

        /// <summary>
        /// Seed groups into the provided instances.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instances"></param>
        private void SeedGroups(AppDbContext context, IEnumerable<Instance> instances)
        {
            foreach (var instance in instances)
            {
                var groups = new List<Group>();
                for (var i = 0; i < 5; i++)
                {
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
        }

        /// <summary>
        /// Seed students into the provided groups.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="groups"></param>
        private void SeedStudents(AppDbContext context, IEnumerable<Group> groups)
        {
            foreach (var group in groups)
            {
                var students = new List<Student>();
                var max = new Random().Next(5, 100);
                for (var i = 0; i <= max; i++)
                {
                    var accountNumber = $"{group.Id}{i}".PadLeft(10, '0');
                    var student = new Student
                    {
                        AccountNumber = accountNumber,
                        Group = group,
                        Password = "student",
                        Email = $"student{i}@group{group.Id}.domain.tld",
                        FirstName = "Student",
                        LastName = $"{i}"
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
            foreach (var student in students)
            {
                var share = new Share
                {
                    Student = student,
                    Balance = Money.FromCurrency(0),
                    ShareType = _shareType,
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
            foreach (var share in shares)
            {
                var max = new Random().Next(5, 500);
                for (var i = 0; i <= max; i++)
                {
                    var amount = new Random().Next(1, 50000); // between $0.01 and $500.00
                    amount *= new Random().Next(0, 2) == 1 ? 1 : -1;
                    var oAmount = Money.FromDatabase(amount);
                    var newBalance = share.Balance + oAmount;

                    var transaction = new Transaction
                    {
                        EffectiveDate = DateTime.UtcNow,
                        Amount = oAmount,
                        TargetShare = share,
                        NewBalance = newBalance
                    };

                    Console.WriteLine($"Share {share.Id}: ${amount} ({oAmount.Amount})");

                    share.Balance = newBalance;
                    context.Add(transaction);
                }
            }

            context.SaveChanges();
        }
    }
}
