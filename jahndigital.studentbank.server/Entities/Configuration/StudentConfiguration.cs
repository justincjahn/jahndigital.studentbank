using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace jahndigital.studentbank.server.Entities.Configuration
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.Property(x => x.Password)
                .HasField("_password")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
