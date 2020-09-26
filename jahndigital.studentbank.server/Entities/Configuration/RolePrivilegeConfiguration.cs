using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace jahndigital.studentbank.server.Entities.Configuration
{
    public class RolePrivilegeConfiguration : IEntityTypeConfiguration<RolePrivilege>
    {
        public void Configure(EntityTypeBuilder<RolePrivilege> builder)
        {
            builder.HasKey(x => new { x.RoleId, x.PrivilegeId });

            builder.HasOne(x => x.Role)
                .WithMany(x => x.RolePrivileges)
                .HasForeignKey(x => x.RoleId);

            builder.HasOne(x => x.Privilege)
                .WithMany(x => x.RolePrivileges)
                .HasForeignKey(x => x.PrivilegeId);
        }
    }
}
