using jahndigital.studentbank.dal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace jahndigital.studentbank.dal.Configuration
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
