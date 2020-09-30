using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace jahndigital.studentbank.dal.Contexts
{
    public class SqliteDbContextDesigner : IDesignTimeDbContextFactory<SqliteDbContext>
    {
        SqliteDbContext IDesignTimeDbContextFactory<SqliteDbContext>.CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder();
            builder.UseSqlServer("Data Source=studentbankdev.sqlite3");
            return new SqliteDbContext(builder.Options);
        }
    }
}
