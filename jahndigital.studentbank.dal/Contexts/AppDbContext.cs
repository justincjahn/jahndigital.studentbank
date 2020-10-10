using jahndigital.studentbank.dal.Entities;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.dal.Contexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options): base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
           modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

        public DbSet<Group> Groups { get; set; }
        public DbSet<Instance> Instances { get; set; }
        public DbSet<Privilege> Privileges { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePrivilege> RolePrivileges { get; set; }
        public DbSet<Share> Shares { get; set; }
        public DbSet<ShareType> ShareTypes { get; set; }
        public DbSet<ShareTypeInstance> ShareTypeInstances { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<StockHistory> StockHistory { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentStock> StudentStocks { get; set; }
        public DbSet<StudentStockHistory> StudentStockHistory { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<StudentPurchase> StudentPurchases { get; set; }
        public DbSet<StudentPurchaseItem> StudentPurchaseItems { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductGroup> ProductGroups { get; set; }
        public DbSet<ProductImage> ProductImages {get; set;}
    }
}
