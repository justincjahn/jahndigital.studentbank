using JahnDigital.StudentBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JahnDigital.StudentBank.Infrastructure.Persistence.Configuration
{
    public class ProductInstanceConfiguration : IEntityTypeConfiguration<ProductInstance>
    {
        public void Configure(EntityTypeBuilder<ProductInstance> builder)
        {
            builder.HasKey(x => new { x.InstanceId, x.ProductId });
        }
    }
}
