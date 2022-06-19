using JahnDigital.StudentBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JahnDigital.StudentBank.Infrastructure.Persistence.Configuration
{
    public class ShareTypeInstanceConfiguration : IEntityTypeConfiguration<ShareTypeInstance>
    {
        public void Configure(EntityTypeBuilder<ShareTypeInstance> builder)
        {
            builder.HasKey(x => new { x.InstanceId, x.ShareTypeId });
        }
    }
}
