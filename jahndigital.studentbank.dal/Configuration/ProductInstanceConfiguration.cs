using jahndigital.studentbank.dal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace jahndigital.studentbank.dal.Configuration
{
    public class ProductInstanceConfiguration : IEntityTypeConfiguration<ProductInstance>
    {
        public void Configure(EntityTypeBuilder<ProductInstance> builder)
        {
            builder.HasKey(x => new {x.InstanceId, x.ProductId});

            builder.HasOne(x => x.Instance)
                .WithMany(x => x.ProductInstances)
                .HasForeignKey(x => x.InstanceId);

            builder.HasOne(x => x.Product)
                .WithMany(x => x.ProductInstances)
                .HasForeignKey(x => x.ProductId);
        }
    }
}