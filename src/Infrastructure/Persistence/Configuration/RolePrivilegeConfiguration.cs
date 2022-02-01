using JahnDigital.StudentBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JahnDigital.StudentBank.Infrastructure.Persistence.Configuration
{
    public class RolePrivilegeConfiguration : IEntityTypeConfiguration<RolePrivilege>
    {
        public void Configure(EntityTypeBuilder<RolePrivilege> builder)
        {
            builder.HasKey(x => new { x.RoleId, x.PrivilegeId });
        }
    }
}
