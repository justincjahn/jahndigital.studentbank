using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JahnDigital.StudentBank.Infrastructure.Persistence.Configuration;

public class StudentPurchasesConfiguration : IEntityTypeConfiguration<StudentPurchase>
{
    public void Configure(EntityTypeBuilder<StudentPurchase> builder)
    {
        builder
            .Property(x => x.RawTotalCost)
            .HasColumnName("TotalCost")
            .IsRequired();

        builder.Ignore(x => x.TotalCost);

        builder
            .Property(x => x.Status)
            .HasColumnType("nvarchar(32)")
            .IsRequired();

        builder
            .Property(x => x.DateCreated)
            .IsDateTimeKind(DateTimeKind.Utc);
    }
}
