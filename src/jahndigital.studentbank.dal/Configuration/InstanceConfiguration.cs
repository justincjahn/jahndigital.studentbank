using jahndigital.studentbank.dal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace jahndigital.studentbank.dal.Configuration
{
    public class InstanceConfiguration : IEntityTypeConfiguration<Instance>
    {
        public void Configure(EntityTypeBuilder<Instance> builder)
        {
            builder.HasIndex(x => x.Description).IsUnique();
            builder.HasIndex(x => x.InviteCode).IsUnique();
        }
    }
}
