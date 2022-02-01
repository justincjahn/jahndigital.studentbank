using JahnDigital.StudentBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JahnDigital.StudentBank.Infrastructure.Persistence.Configuration
{
    public class StockInstanceConfiguration : IEntityTypeConfiguration<StockInstance>
    {
        public void Configure(EntityTypeBuilder<StockInstance> builder)
        {
            builder.HasKey(x => new { x.InstanceId, x.StockId });
        }
    }
}
