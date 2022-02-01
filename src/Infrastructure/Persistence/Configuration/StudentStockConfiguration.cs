using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JahnDigital.StudentBank.Infrastructure.Persistence.Configuration;

public class StudentStockConfiguration : IEntityTypeConfiguration<StudentStock>
{
    public void Configure(EntityTypeBuilder<StudentStock> builder)
    {
        builder
            .Property(x => x.SharesOwned)
            .IsRequired();

        builder
            .Property(x => x.RawNetContribution)
            .HasColumnName("NetContribution")
            .IsRequired();

        builder.Ignore(x => x.NetContribution);

        builder
            .Property(x => x.DateLastActive)
            .IsDateTimeKind(DateTimeKind.Utc);

        builder
            .Property(x => x.DateCreated)
            .IsDateTimeKind(DateTimeKind.Utc);
    }
}
