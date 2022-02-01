using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JahnDigital.StudentBank.Infrastructure.Persistence.Configuration;

public class ShareTypeConfiguration : IEntityTypeConfiguration<ShareType>
{
    public void Configure(EntityTypeBuilder<ShareType> builder)
    {
        builder
            .Property(x => x.Name)
            .HasMaxLength(32)
            .IsRequired();

        builder
            .Property(x => x.RawDividendRate)
            .HasColumnName("DividendRate")
            .IsRequired();

        builder.Ignore(x => x.DividendRate);

        builder
            .Property(x => x.WithdrawalLimitCount)
            .IsRequired();

        builder
            .Property(x => x.WithdrawalLimitPeriod)
            .IsRequired();

        builder
            .Property(x => x.RawWithdrawalLimitFee)
            .HasColumnName("WithdrawalLimitFee")
            .IsRequired();

        builder.Ignore(x => x.WithdrawalLimitFee);

        builder
            .Property(x => x.WithdrawalLimitLastReset)
            .IsDateTimeKind(DateTimeKind.Utc);

        builder
            .Property(x => x.DateCreated)
            .IsDateTimeKind(DateTimeKind.Utc);

        builder
            .Property(x => x.DateDeleted)
            .IsDateTimeKind(DateTimeKind.Utc);
    }
}
