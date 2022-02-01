using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JahnDigital.StudentBank.Infrastructure.Persistence.Configuration
{
    public class GroupConfiguration : IEntityTypeConfiguration<Group>
    {
        public void Configure(EntityTypeBuilder<Group> builder)
        {
            builder
                .HasIndex("Name", "InstanceId")
                .IsUnique();

            builder
                .Property(x => x.Name)
                .HasMaxLength(32)
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
