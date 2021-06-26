using jahndigital.studentbank.dal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace jahndigital.studentbank.dal.Configuration
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.Property(x => x.Password)
                .HasField("_password")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
            
            builder.HasIndex(new string[] {"AccountNumber", "GroupId"}).IsUnique();
        }
    }
}
