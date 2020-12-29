using System;
using System.Reflection;
using jahndigital.studentbank.dal.Annotations;
using jahndigital.studentbank.dal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace jahndigital.studentbank.dal.Contexts
{
    public class AppDbContext : DbContext
    {
        public DbSet<Group> Groups => Set<Group>();
        public DbSet<Instance> Instances => Set<Instance>();
        public DbSet<Privilege> Privileges => Set<Privilege>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<RolePrivilege> RolePrivileges => Set<RolePrivilege>();
        public DbSet<Share> Shares => Set<Share>();
        public DbSet<ShareType> ShareTypes => Set<ShareType>();
        public DbSet<ShareTypeInstance> ShareTypeInstances => Set<ShareTypeInstance>();
        public DbSet<Stock> Stocks => Set<Stock>();
        public DbSet<StockHistory> StockHistory => Set<StockHistory>();
        public DbSet<StockInstance> StockInstances => Set<StockInstance>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<StudentStock> StudentStocks => Set<StudentStock>();
        public DbSet<StudentStockHistory> StudentStockHistory => Set<StudentStockHistory>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<User> Users => Set<User>();
        public DbSet<StudentPurchase> StudentPurchases => Set<StudentPurchase>();
        public DbSet<StudentPurchaseItem> StudentPurchaseItems => Set<StudentPurchaseItem>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductInstance> ProductInstances => Set<ProductInstance>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();

        public AppDbContext(DbContextOptions options): base(options) { }

        /// <summary>
        /// Closure that generates a <see cref="ValueConverter{TModel, TProvider}"/> for dates.
        /// </summary>
        /// <param name="kind">The date kind.</param>
        /// <returns></returns>
        private ValueConverter<DateTime, DateTime> _convert(DateTimeKind kind)
        {
            return new ValueConverter<DateTime, DateTime>(x => x, x => DateTime.SpecifyKind(x, kind));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

            // Loop through all the model properties and apply a converter if there is a DateTimeKindAttribute
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()) {
                foreach (var property in entityType.GetProperties()) {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?)) {
                        var kind = property.DateTimeKind();
                        if (kind == DateTimeKind.Unspecified) continue; // We can make all dates UTC by default here
                        property.SetValueConverter(_convert(kind));
                    }
                }
            }
        }
    }
}
