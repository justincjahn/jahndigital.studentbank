using JahnDigital.StudentBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace JahnDigital.StudentBank.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Group> Groups { get; }
    
    DbSet<Instance> Instances { get; }
    
    DbSet<Privilege> Privileges { get; }
    
    DbSet<Role> Roles { get; }
    
    DbSet<RolePrivilege> RolePrivileges { get; }
    
    DbSet<Share> Shares { get; }
    
    DbSet<ShareType> ShareTypes { get; }
    
    DbSet<ShareTypeInstance> ShareTypeInstances { get; }
    
    DbSet<Stock> Stocks { get; }
    
    DbSet<StockHistory> StockHistory { get; }
    
    DbSet<StockInstance> StockInstances { get; }
    
    DbSet<Student> Students { get; }
    
    DbSet<StudentStock> StudentStocks { get; }
    
    DbSet<StudentStockHistory> StudentStockHistory { get; }
    
    DbSet<Transaction> Transactions { get; }
    
    DbSet<User> Users { get; }
    
    DbSet<StudentPurchase> StudentPurchases { get; }
    
    DbSet<StudentPurchaseItem> StudentPurchaseItems { get; }
    
    DbSet<Product> Products { get; }
    
    DbSet<ProductInstance> ProductInstances { get; }
    
    DbSet<ProductImage> ProductImages { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
