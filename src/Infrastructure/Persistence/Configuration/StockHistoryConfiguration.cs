using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JahnDigital.StudentBank.Infrastructure.Persistence.Configuration;

public class StockHistoryConfiguration : IEntityTypeConfiguration<StockHistory>
{
    public void Configure(EntityTypeBuilder<StockHistory> builder)
    {
        builder
            .Property(x => x.RawValue)
            .HasColumnName("Value")
            .IsRequired();

        builder.Ignore(x => x.Value);

        builder
            .Property(x => x.DateChanged)
            .IsDateTimeKind(DateTimeKind.Utc)
            .IsRequired();
    }
}
