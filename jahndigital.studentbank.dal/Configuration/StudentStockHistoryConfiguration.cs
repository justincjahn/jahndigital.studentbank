using jahndigital.studentbank.dal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace jahndigital.studentbank.dal.Configuration
{
    public class StudentStockHistoryConfiguration : IEntityTypeConfiguration<StudentStockHistory>
    {
        public void Configure(EntityTypeBuilder<StudentStockHistory> builder)
        {
            builder
                .HasOne(x => x.Transaction)
                .WithOne()
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
