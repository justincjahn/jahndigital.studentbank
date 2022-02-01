using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JahnDigital.StudentBank.Infrastructure.Persistence.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder
            .Property(x => x.Quantity)
            .HasField("_quantity")
            .IsRequired();

        builder
            .Property(x => x.RawCost)
            .HasColumnName("Cost")
            .IsRequired();

        builder
            .Ignore(x => x.Cost);

        builder
            .Property(x => x.Quantity)
            .IsRequired();

        builder
            .Property(x => x.DateCreated)
            .IsDateTimeKind(DateTimeKind.Utc);

        builder
            .Property(x => x.DateDeleted)
            .IsDateTimeKind(DateTimeKind.Utc);

        builder
            .Property(x => x.Name)
            .HasMaxLength(128)
            .IsRequired();

        builder
            .Property(x => x.Description)
            .HasMaxLength(256);

        builder
            .OwnsMany(x => x.Images, p =>
            {
                p.HasKey(x => x.Id);

                p
                    .Property(x => x.Url)
                    .HasMaxLength(256)
                    .IsRequired();
            });
    }
}
