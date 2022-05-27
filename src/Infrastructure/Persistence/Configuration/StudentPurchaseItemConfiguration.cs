using JahnDigital.StudentBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JahnDigital.StudentBank.Infrastructure.Persistence.Configuration;

public class StudentPurchaseItemConfiguration : IEntityTypeConfiguration<StudentPurchaseItem>
{
    public void Configure(EntityTypeBuilder<StudentPurchaseItem> builder)
    {
        builder
            .Property(x => x.RawPurchasePrice)
            .HasColumnName("PurchasePrice")
            .IsRequired();

        builder.Ignore(x => x.PurchasePrice);

        builder
            .Property(x => x.RawTotalPurchasePrice)
            .HasColumnName("TotalPurchasePrice")
            .IsRequired();

        builder.Ignore(x => x.TotalPurchasePrice);
    }
}
