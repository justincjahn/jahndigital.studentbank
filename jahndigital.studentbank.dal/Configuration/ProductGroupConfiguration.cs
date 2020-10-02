using jahndigital.studentbank.dal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace jahndigital.studentbank.dal.Configuration
{
    public class ProductGroupConfiguration : IEntityTypeConfiguration<ProductGroup>
    {
        public void Configure(EntityTypeBuilder<ProductGroup> builder)
        {
            builder.HasKey(x => new { x.GroupId, x.ProductId });

            builder.HasOne(x => x.Group)
                .WithMany(x => x.ProductGroups)
                .HasForeignKey(x => x.GroupId);

            builder.HasOne(x => x.Product)
                .WithMany(x => x.ProductGroups)
                .HasForeignKey(x => x.ProductId);
        }
    }
}
