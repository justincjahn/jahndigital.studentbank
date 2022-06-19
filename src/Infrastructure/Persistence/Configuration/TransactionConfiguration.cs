using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JahnDigital.StudentBank.Infrastructure.Persistence.Configuration;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder
            .Property(x => x.TransactionType)
            .HasMaxLength(1)
            .IsRequired();

        builder
            .Property(x => x.Comment)
            .HasMaxLength(255);

        builder
            .Property(x => x.RawAmount)
            .HasColumnName("Amount")
            .IsRequired();

        builder.Ignore(x => x.Amount);

        builder
            .Property(x => x.RawNewBalance)
            .HasColumnName("NewBalance")
            .IsRequired();

        builder.Ignore(x => x.NewBalance);

        builder
            .Property(x => x.EffectiveDate)
            .IsDateTimeKind(DateTimeKind.Utc)
            .IsRequired();
    }
}
