using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JahnDigital.StudentBank.Infrastructure.Persistence.Configuration
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.Property(x => x.Password)
                .HasMaxLength(84)
                .IsRequired();

            builder.HasIndex("AccountNumber", "GroupId").IsUnique();

            builder
                .Property(x => x.AccountNumber)
                .HasMaxLength(10)
                .IsRequired();

            builder
                .Property(x => x.Email)
                .HasMaxLength(64);

            builder
                .Property(x => x.FirstName)
                .HasMaxLength(64)
                .IsRequired();

            builder
                .Property(x => x.LastName)
                .HasMaxLength(64)
                .IsRequired();

            builder
                .OwnsMany(x => x.RefreshTokens, o =>
                {
                    o.HasKey(x => x.Id);

                    o
                        .Property(x => x.Token)
                        .HasMaxLength(7168);

                    o
                        .Property(x => x.Created)
                        .IsDateTimeKind(DateTimeKind.Utc);

                    o
                        .Property(x => x.Expires)
                        .IsDateTimeKind(DateTimeKind.Utc);

                    o
                        .Property(x => x.Expires)
                        .IsDateTimeKind(DateTimeKind.Utc);

                    o
                        .Property(x => x.Revoked)
                        .IsDateTimeKind(DateTimeKind.Utc);

                    o.Ignore(x => x.IsExpired);

                    o
                        .Property(x => x.CreatedByIpAddress)
                        .HasMaxLength(39);

                    o
                        .Property(x => x.RevokedByIpAddress)
                        .HasMaxLength(39);

                    o
                        .Property(x => x.ReplacedByToken)
                        .HasMaxLength(7168);

                    o.Ignore(x => x.IsActive);
                });

            builder
                .Property(x => x.DateLastLogin)
                .IsDateTimeKind(DateTimeKind.Utc);

            builder
                .Property(x => x.DateCreated)
                .IsDateTimeKind(DateTimeKind.Utc);

            builder
                .Property(x => x.DateRegistered)
                .IsDateTimeKind(DateTimeKind.Utc);

            builder
                .Property(x => x.DateDeleted)
                .IsDateTimeKind(DateTimeKind.Utc);
        }
    }
}
