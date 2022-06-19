using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JahnDigital.StudentBank.Infrastructure.Persistence;

public class AppDbContextDesigner : IDesignTimeDbContextFactory<AppDbContext>
{
    AppDbContext IDesignTimeDbContextFactory<AppDbContext>.CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder? builder = new();
        builder.UseSqlServer("Data Source=(local);Initial Catalog=test_studentbank; Integrated Security=True;");
        return new AppDbContext(builder.Options);
    }
}
