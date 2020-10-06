﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using jahndigital.studentbank.dal.Contexts;

namespace jahndigital.studentbank.dal.Migrations.mssql
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.Group", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateDeleted")
                        .HasColumnType("datetime2");

                    b.Property<long>("InstanceId")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(32)")
                        .HasMaxLength(32);

                    b.HasKey("Id");

                    b.HasIndex("InstanceId");

                    b.HasIndex("Name", "InstanceId")
                        .IsUnique();

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.Instance", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateDeleted")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(32)")
                        .HasMaxLength(32);

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("Description")
                        .IsUnique();

                    b.ToTable("Instances");
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.Privilege", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(16)")
                        .HasMaxLength(16);

                    b.HasKey("Id");

                    b.ToTable("Privileges");
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.Product", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateDeleted")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<bool>("IsLimitedQuantity")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<long>("RawCost")
                        .HasColumnName("Cost")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.ProductGroup", b =>
                {
                    b.Property<long>("GroupId")
                        .HasColumnType("bigint");

                    b.Property<long>("ProductId")
                        .HasColumnType("bigint");

                    b.HasKey("GroupId", "ProductId");

                    b.HasIndex("ProductId");

                    b.ToTable("ProductGroups");
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.Role", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<bool>("IsBuiltIn")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(32)")
                        .HasMaxLength(32);

                    b.HasKey("Id");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.RolePrivilege", b =>
                {
                    b.Property<long>("RoleId")
                        .HasColumnType("bigint");

                    b.Property<long>("PrivilegeId")
                        .HasColumnType("bigint");

                    b.HasKey("RoleId", "PrivilegeId");

                    b.HasIndex("PrivilegeId");

                    b.ToTable("RolePrivileges");
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.Share", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateDeleted")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DateLastActive")
                        .HasColumnType("datetime2");

                    b.Property<long>("RawBalance")
                        .HasColumnName("Balance")
                        .HasColumnType("bigint");

                    b.Property<long>("ShareTypeId")
                        .HasColumnType("bigint");

                    b.Property<long>("StudentId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("ShareTypeId");

                    b.HasIndex("StudentId");

                    b.ToTable("Shares");
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.ShareType", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateDeleted")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(32)")
                        .HasMaxLength(32);

                    b.Property<long>("RawDividendRate")
                        .HasColumnName("DividendRate")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("ShareTypes");
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.Stock", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("AvailableShares")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateDeleted")
                        .HasColumnType("datetime2");

                    b.Property<long>("InstanceId")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(32)")
                        .HasMaxLength(32);

                    b.Property<long>("RawCurrentValue")
                        .HasColumnName("CurrentValue")
                        .HasColumnType("bigint");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasColumnType("nvarchar(10)")
                        .HasMaxLength(10);

                    b.Property<long>("TotalShares")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("InstanceId");

                    b.ToTable("Stocks");
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.StockHistory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateChanged")
                        .HasColumnType("datetime2");

                    b.Property<long>("RawValue")
                        .HasColumnName("Value")
                        .HasColumnType("bigint");

                    b.Property<long>("StockId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("StockId");

                    b.ToTable("StockHistory");
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.Student", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AccountNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(10)")
                        .HasMaxLength(10);

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateDeleted")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<long>("GroupId")
                        .HasColumnType("bigint");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(84)")
                        .HasMaxLength(84);

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.HasIndex("AccountNumber", "GroupId")
                        .IsUnique();

                    b.ToTable("Students");
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.StudentPurchase", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<long>("RawTotalCost")
                        .HasColumnName("TotalCost")
                        .HasColumnType("bigint");

                    b.Property<long>("StudentId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("StudentId");

                    b.ToTable("StudentPurchases");
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.StudentPurchaseItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("ProductId")
                        .HasColumnType("bigint");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<long>("RawPurchasePrice")
                        .HasColumnName("PurchasePrice")
                        .HasColumnType("bigint");

                    b.Property<long>("StudentPurchaseId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("ProductId");

                    b.HasIndex("StudentPurchaseId");

                    b.ToTable("StudentPurchaseItems");
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.StudentStock", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DateLastActive")
                        .HasColumnType("datetime2");

                    b.Property<long>("StockId")
                        .HasColumnType("bigint");

                    b.Property<long>("StudentId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("StockId");

                    b.HasIndex("StudentId");

                    b.ToTable("StudentStocks");
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.StudentStockHistory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Count")
                        .HasColumnType("int");

                    b.Property<DateTime>("DatePosted")
                        .HasColumnType("datetime2");

                    b.Property<long>("RawAmount")
                        .HasColumnName("Amount")
                        .HasColumnType("bigint");

                    b.Property<long>("StudentStockId")
                        .HasColumnType("bigint");

                    b.Property<long>("TransactionId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("StudentStockId");

                    b.HasIndex("TransactionId");

                    b.ToTable("StudentStockHistory");
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.Transaction", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("EffectiveDate")
                        .HasColumnType("datetime2");

                    b.Property<long>("RawAmount")
                        .HasColumnName("Amount")
                        .HasColumnType("bigint");

                    b.Property<long>("RawNewBalance")
                        .HasColumnName("NewBalance")
                        .HasColumnType("bigint");

                    b.Property<long>("TargetShareId")
                        .HasColumnType("bigint");

                    b.Property<string>("TransactionType")
                        .IsRequired()
                        .HasColumnType("nvarchar(1)")
                        .HasMaxLength(1);

                    b.HasKey("Id");

                    b.HasIndex("TargetShareId");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateDeleted")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(84)")
                        .HasMaxLength(84);

                    b.Property<long>("RoleId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.Group", b =>
                {
                    b.HasOne("jahndigital.studentbank.dal.Entities.Instance", "Instance")
                        .WithMany("Groups")
                        .HasForeignKey("InstanceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.Product", b =>
                {
                    b.OwnsMany("jahndigital.studentbank.dal.Entities.ProductImages", "Images", b1 =>
                        {
                            b1.Property<long>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("bigint")
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b1.Property<long>("ProductId")
                                .HasColumnType("bigint");

                            b1.Property<string>("Url")
                                .IsRequired()
                                .HasColumnType("nvarchar(256)")
                                .HasMaxLength(256);

                            b1.HasKey("Id");

                            b1.HasIndex("ProductId");

                            b1.ToTable("ProductImages");

                            b1.WithOwner()
                                .HasForeignKey("ProductId");
                        });
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.ProductGroup", b =>
                {
                    b.HasOne("jahndigital.studentbank.dal.Entities.Group", "Group")
                        .WithMany("ProductGroups")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("jahndigital.studentbank.dal.Entities.Product", "Product")
                        .WithMany("ProductGroups")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.RolePrivilege", b =>
                {
                    b.HasOne("jahndigital.studentbank.dal.Entities.Privilege", "Privilege")
                        .WithMany("RolePrivileges")
                        .HasForeignKey("PrivilegeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("jahndigital.studentbank.dal.Entities.Role", "Role")
                        .WithMany("RolePrivileges")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.Share", b =>
                {
                    b.HasOne("jahndigital.studentbank.dal.Entities.ShareType", "ShareType")
                        .WithMany("Shares")
                        .HasForeignKey("ShareTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("jahndigital.studentbank.dal.Entities.Student", "Student")
                        .WithMany("Shares")
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.Stock", b =>
                {
                    b.HasOne("jahndigital.studentbank.dal.Entities.Instance", "Instance")
                        .WithMany("Stocks")
                        .HasForeignKey("InstanceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.StockHistory", b =>
                {
                    b.HasOne("jahndigital.studentbank.dal.Entities.Stock", "Stock")
                        .WithMany("History")
                        .HasForeignKey("StockId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.Student", b =>
                {
                    b.HasOne("jahndigital.studentbank.dal.Entities.Group", "Group")
                        .WithMany("Students")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsMany("jahndigital.studentbank.dal.Entities.RefreshToken", "RefreshTokens", b1 =>
                        {
                            b1.Property<long>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("bigint")
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b1.Property<DateTime>("Created")
                                .HasColumnType("datetime2");

                            b1.Property<string>("CreatedByIpAddress")
                                .HasColumnType("nvarchar(39)")
                                .HasMaxLength(39);

                            b1.Property<DateTime>("Expires")
                                .HasColumnType("datetime2");

                            b1.Property<string>("ReplacedByToken")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<DateTime?>("Revoked")
                                .HasColumnType("datetime2");

                            b1.Property<string>("RevokedByIpAddress")
                                .HasColumnType("nvarchar(39)")
                                .HasMaxLength(39);

                            b1.Property<long>("StudentId")
                                .HasColumnType("bigint");

                            b1.Property<string>("Token")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("Id");

                            b1.HasIndex("StudentId");

                            b1.ToTable("Students_RefreshTokens");

                            b1.WithOwner()
                                .HasForeignKey("StudentId");
                        });
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.StudentPurchase", b =>
                {
                    b.HasOne("jahndigital.studentbank.dal.Entities.Student", "Student")
                        .WithMany("Purchases")
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.StudentPurchaseItem", b =>
                {
                    b.HasOne("jahndigital.studentbank.dal.Entities.Product", "Product")
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("jahndigital.studentbank.dal.Entities.StudentPurchase", "StudentPurchase")
                        .WithMany("Items")
                        .HasForeignKey("StudentPurchaseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.StudentStock", b =>
                {
                    b.HasOne("jahndigital.studentbank.dal.Entities.Stock", "Stock")
                        .WithMany("StudentStock")
                        .HasForeignKey("StockId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("jahndigital.studentbank.dal.Entities.Student", "Student")
                        .WithMany()
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.StudentStockHistory", b =>
                {
                    b.HasOne("jahndigital.studentbank.dal.Entities.StudentStock", "StudentStock")
                        .WithMany("History")
                        .HasForeignKey("StudentStockId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("jahndigital.studentbank.dal.Entities.Transaction", "Transaction")
                        .WithMany()
                        .HasForeignKey("TransactionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.Transaction", b =>
                {
                    b.HasOne("jahndigital.studentbank.dal.Entities.Share", "TargetShare")
                        .WithMany("Transactions")
                        .HasForeignKey("TargetShareId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("jahndigital.studentbank.dal.Entities.User", b =>
                {
                    b.HasOne("jahndigital.studentbank.dal.Entities.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsMany("jahndigital.studentbank.dal.Entities.RefreshToken", "RefreshTokens", b1 =>
                        {
                            b1.Property<long>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("bigint")
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b1.Property<DateTime>("Created")
                                .HasColumnType("datetime2");

                            b1.Property<string>("CreatedByIpAddress")
                                .HasColumnType("nvarchar(39)")
                                .HasMaxLength(39);

                            b1.Property<DateTime>("Expires")
                                .HasColumnType("datetime2");

                            b1.Property<string>("ReplacedByToken")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<DateTime?>("Revoked")
                                .HasColumnType("datetime2");

                            b1.Property<string>("RevokedByIpAddress")
                                .HasColumnType("nvarchar(39)")
                                .HasMaxLength(39);

                            b1.Property<string>("Token")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<long>("UserId")
                                .HasColumnType("bigint");

                            b1.HasKey("Id");

                            b1.HasIndex("UserId");

                            b1.ToTable("Users_RefreshTokens");

                            b1.WithOwner()
                                .HasForeignKey("UserId");
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
