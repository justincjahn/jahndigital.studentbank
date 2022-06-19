using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JahnDigital.StudentBank.Infrastructure.Persistence.Configuration
{
    public class StudentStockHistoryConfiguration : IEntityTypeConfiguration<StudentStockHistory>
    {
        public void Configure(EntityTypeBuilder<StudentStockHistory> builder)
        {
            builder
                .HasOne(x => x.Transaction)
                .WithOne()
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .Property(x => x.Count)
                .IsRequired();

            builder
                .Property(x => x.RawAmount)
                .HasColumnName("Amount")
                .IsRequired();

            builder.Ignore(x => x.Amount);

            builder
                .Property(x => x.DatePosted)
                .IsDateTimeKind(DateTimeKind.Utc)
                .IsRequired();
        }
    }
}
