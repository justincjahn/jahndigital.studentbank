using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace jahndigital.studentbank.dal.Contexts
{
    public class AppDbContextDesigner : IDesignTimeDbContextFactory<AppDbContext>
    {
        AppDbContext IDesignTimeDbContextFactory<AppDbContext>.CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder();
            builder.UseSqlServer("Data Source=.\\SQLExpress;Initial Catalog=studentbankdev; Integrated Security=True;");

            return new AppDbContext(builder.Options);
        }
    }
}