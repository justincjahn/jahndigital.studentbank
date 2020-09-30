using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.dal.Contexts
{
    public class SqliteDbContext : AppDbContext
    {
        public SqliteDbContext(DbContextOptions options) : base(options) { }
    }
}
