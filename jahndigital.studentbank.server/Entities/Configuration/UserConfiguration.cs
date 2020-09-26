using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace jahndigital.studentbank.server.Entities.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(x => x.Password)
                .HasField("_password")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
