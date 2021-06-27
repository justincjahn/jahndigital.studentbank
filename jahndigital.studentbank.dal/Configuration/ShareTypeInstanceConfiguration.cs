using jahndigital.studentbank.dal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace jahndigital.studentbank.dal.Configuration
{
    public class ShareTypeInstanceConfiguration : IEntityTypeConfiguration<ShareTypeInstance>
    {
        public void Configure(EntityTypeBuilder<ShareTypeInstance> builder)
        {
            builder.HasKey(x => new {x.InstanceId, x.ShareTypeId});

            builder.HasOne(x => x.Instance)
                .WithMany(x => x.ShareTypeInstances)
                .HasForeignKey(x => x.InstanceId);

            builder.HasOne(x => x.ShareType)
                .WithMany(x => x.ShareTypeInstances)
                .HasForeignKey(x => x.ShareTypeId);
        }
    }
}