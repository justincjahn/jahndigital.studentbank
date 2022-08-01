using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JahnDigital.StudentBank.Infrastructure.Persistence.Configuration;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder
            .Property(x => x.Symbol)
            .HasMaxLength(10)
            .IsRequired();

        builder
            .Property(x => x.Name)
            .HasMaxLength(32)
            .IsRequired();

        builder
            .Property(x => x.RawCurrentValue)
            .HasColumnName("CurrentValue")
            .IsRequired();

        builder.Ignore(x => x.CurrentValue);

        builder
            .Property(x => x.DateCreated)
            .IsDateTimeKind(DateTimeKind.Utc);

        builder
            .Property(x => x.DateDeleted)
            .IsDateTimeKind(DateTimeKind.Utc);
    }
}
