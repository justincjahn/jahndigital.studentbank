using jahndigital.studentbank.dal.Entities;
using Microsoft.EntityFrameworkCore;

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

        public AppDbContext(DbContextOptions options): base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
           modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
