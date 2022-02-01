using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JahnDigital.StudentBank.Infrastructure.Persistence.Configuration;

public class ShareConfiguration : IEntityTypeConfiguration<Share>
{
    public void Configure(EntityTypeBuilder<Share> builder)
    {
        builder
            .Property(x => x.RawBalance)
            .HasColumnName("Balance")
            .IsRequired();

        builder.Ignore(x => x.Balance);

        builder
            .Property(x => x.RawDividendLastAmount)
            .HasColumnName("DividendLastAmount")
            .IsRequired();

        builder.Ignore(x => x.DividendLastAmount);

        builder
            .Property(x => x.RawTotalDividends)
            .HasColumnName("TotalDividends")
            .IsRequired();

        builder.Ignore(x => x.TotalDividends);

        builder
            .Property(x => x.LimitedWithdrawalCount)
            .IsRequired();

        builder
            .Property(x => x.DateLastActive)
            .IsDateTimeKind(DateTimeKind.Utc)
            .IsRequired();

        builder
            .Property(x => x.DateCreated)
            .IsDateTimeKind(DateTimeKind.Utc);

        builder
            .Property(x => x.DateDeleted)
            .IsDateTimeKind(DateTimeKind.Utc);
    }
}
