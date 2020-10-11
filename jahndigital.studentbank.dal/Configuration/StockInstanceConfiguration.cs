using jahndigital.studentbank.dal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace jahndigital.studentbank.dal.Configuration
{
    public class StockInstanceConfiguration : IEntityTypeConfiguration<StockInstance>
    {
        public void Configure(EntityTypeBuilder<StockInstance> builder)
        {
            builder.HasKey(x => new { x.InstanceId, x.StockId });

            builder.HasOne(x => x.Instance)
                .WithMany(x => x.StockInstances)
                .HasForeignKey(x => x.InstanceId);

            builder.HasOne(x => x.Stock)
                .WithMany(x => x.StockInstances)
                .HasForeignKey(x => x.StockId);
        }
    }
}