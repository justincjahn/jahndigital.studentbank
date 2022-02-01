using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JahnDigital.StudentBank.Infrastructure.Persistence.Configuration
{
    public class InstanceConfiguration : IEntityTypeConfiguration<Instance>
    {
        public void Configure(EntityTypeBuilder<Instance> builder)
        {
            builder.HasIndex(x => x.Description).IsUnique();
            
            builder.HasIndex(x => x.InviteCode).IsUnique();
            
            builder
                .Property(x => x.Description)
                .HasMaxLength(32)
                .IsRequired();

            builder
                .Property(x => x.IsActive)
                .IsRequired();

            builder
                .Property(x => x.InviteCode)
                .HasMaxLength(38)
                .IsRequired();

            builder
                .Property(x => x.DateCreated)
                .IsDateTimeKind(DateTimeKind.Utc);

            builder
                .Property(x => x.DateDeleted)
                .IsDateTimeKind(DateTimeKind.Utc);
        }
    }
}
