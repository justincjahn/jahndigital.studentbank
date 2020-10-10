using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace jahndigital.studentbank.dal.Migrations.mssql
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Instances",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateDeleted = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Privileges",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 16, nullable: false),
                    Description = table.Column<string>(maxLength: 128, nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Privileges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 128, nullable: false),
                    Description = table.Column<string>(maxLength: 256, nullable: true),
                    Cost = table.Column<long>(nullable: false),
                    IsLimitedQuantity = table.Column<bool>(nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateDeleted = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 32, nullable: false),
                    Description = table.Column<string>(maxLength: 128, nullable: true),
                    IsBuiltIn = table.Column<bool>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShareTypes",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 32, nullable: false),
                    DividendRate = table.Column<long>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateDeleted = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShareTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 32, nullable: false),
                    InstanceId = table.Column<long>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateDeleted = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groups_Instances_InstanceId",
                        column: x => x.InstanceId,
                        principalTable: "Instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstanceId = table.Column<long>(nullable: false),
                    Symbol = table.Column<string>(maxLength: 10, nullable: false),
                    Name = table.Column<string>(maxLength: 32, nullable: false),
                    TotalShares = table.Column<long>(nullable: false),
                    AvailableShares = table.Column<long>(nullable: false),
                    CurrentValue = table.Column<long>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateDeleted = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stocks_Instances_InstanceId",
                        column: x => x.InstanceId,
                        principalTable: "Instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(maxLength: 256, nullable: false),
                    ProductId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePrivileges",
                columns: table => new
                {
                    RoleId = table.Column<long>(nullable: false),
                    PrivilegeId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePrivileges", x => new { x.RoleId, x.PrivilegeId });
                    table.ForeignKey(
                        name: "FK_RolePrivileges_Privileges_PrivilegeId",
                        column: x => x.PrivilegeId,
                        principalTable: "Privileges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePrivileges_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(maxLength: 64, nullable: false),
                    Password = table.Column<string>(maxLength: 84, nullable: false),
                    RoleId = table.Column<long>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateDeleted = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShareTypeInstances",
                columns: table => new
                {
                    ShareTypeId = table.Column<long>(nullable: false),
                    InstanceId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShareTypeInstances", x => new { x.InstanceId, x.ShareTypeId });
                    table.ForeignKey(
                        name: "FK_ShareTypeInstances_Instances_InstanceId",
                        column: x => x.InstanceId,
                        principalTable: "Instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShareTypeInstances_ShareTypes_ShareTypeId",
                        column: x => x.ShareTypeId,
                        principalTable: "ShareTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductGroups",
                columns: table => new
                {
                    GroupId = table.Column<long>(nullable: false),
                    ProductId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductGroups", x => new { x.GroupId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_ProductGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductGroups_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountNumber = table.Column<string>(maxLength: 10, nullable: false),
                    Email = table.Column<string>(maxLength: 64, nullable: false),
                    FirstName = table.Column<string>(maxLength: 64, nullable: false),
                    LastName = table.Column<string>(maxLength: 64, nullable: false),
                    Password = table.Column<string>(maxLength: 84, nullable: false),
                    GroupId = table.Column<long>(nullable: false),
                    DateDeleted = table.Column<DateTime>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockHistory",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockId = table.Column<long>(nullable: false),
                    Value = table.Column<long>(nullable: false),
                    DateChanged = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockHistory_Stocks_StockId",
                        column: x => x.StockId,
                        principalTable: "Stocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users_RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Expires = table.Column<DateTime>(nullable: false),
                    Revoked = table.Column<DateTime>(nullable: true),
                    CreatedByIpAddress = table.Column<string>(maxLength: 39, nullable: true),
                    RevokedByIpAddress = table.Column<string>(maxLength: 39, nullable: true),
                    ReplacedByToken = table.Column<string>(nullable: true),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Shares",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShareTypeId = table.Column<long>(nullable: false),
                    StudentId = table.Column<long>(nullable: false),
                    Balance = table.Column<long>(nullable: false),
                    DateLastActive = table.Column<DateTime>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateDeleted = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shares_ShareTypes_ShareTypeId",
                        column: x => x.ShareTypeId,
                        principalTable: "ShareTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Shares_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentPurchases",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<long>(nullable: false),
                    TotalCost = table.Column<long>(nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentPurchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentPurchases_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Students_RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Expires = table.Column<DateTime>(nullable: false),
                    Revoked = table.Column<DateTime>(nullable: true),
                    CreatedByIpAddress = table.Column<string>(maxLength: 39, nullable: true),
                    RevokedByIpAddress = table.Column<string>(maxLength: 39, nullable: true),
                    ReplacedByToken = table.Column<string>(nullable: true),
                    StudentId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_RefreshTokens_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentStocks",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<long>(nullable: false),
                    StockId = table.Column<long>(nullable: false),
                    DateLastActive = table.Column<DateTime>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentStocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentStocks_Stocks_StockId",
                        column: x => x.StockId,
                        principalTable: "Stocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentStocks_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionType = table.Column<string>(maxLength: 1, nullable: false),
                    TargetShareId = table.Column<long>(nullable: false),
                    Comment = table.Column<string>(maxLength: 255, nullable: true),
                    Amount = table.Column<long>(nullable: false),
                    NewBalance = table.Column<long>(nullable: false),
                    EffectiveDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Shares_TargetShareId",
                        column: x => x.TargetShareId,
                        principalTable: "Shares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentPurchaseItems",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentPurchaseId = table.Column<long>(nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    ProductId = table.Column<long>(nullable: false),
                    PurchasePrice = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentPurchaseItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentPurchaseItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentPurchaseItems_StudentPurchases_StudentPurchaseId",
                        column: x => x.StudentPurchaseId,
                        principalTable: "StudentPurchases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentStockHistory",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentStockId = table.Column<long>(nullable: false),
                    Count = table.Column<int>(nullable: false),
                    Amount = table.Column<long>(nullable: false),
                    TransactionId = table.Column<long>(nullable: false),
                    DatePosted = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentStockHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentStockHistory_StudentStocks_StudentStockId",
                        column: x => x.StudentStockId,
                        principalTable: "StudentStocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentStockHistory_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Groups_InstanceId",
                table: "Groups",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_Name_InstanceId",
                table: "Groups",
                columns: new[] { "Name", "InstanceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instances_Description",
                table: "Instances",
                column: "Description",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductGroups_ProductId",
                table: "ProductGroups",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePrivileges_PrivilegeId",
                table: "RolePrivileges",
                column: "PrivilegeId");

            migrationBuilder.CreateIndex(
                name: "IX_Shares_ShareTypeId",
                table: "Shares",
                column: "ShareTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Shares_StudentId",
                table: "Shares",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ShareTypeInstances_ShareTypeId",
                table: "ShareTypeInstances",
                column: "ShareTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StockHistory_StockId",
                table: "StockHistory",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_InstanceId",
                table: "Stocks",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPurchaseItems_ProductId",
                table: "StudentPurchaseItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPurchaseItems_StudentPurchaseId",
                table: "StudentPurchaseItems",
                column: "StudentPurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPurchases_StudentId",
                table: "StudentPurchases",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_GroupId",
                table: "Students",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_AccountNumber_GroupId",
                table: "Students",
                columns: new[] { "AccountNumber", "GroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_RefreshTokens_StudentId",
                table: "Students_RefreshTokens",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentStockHistory_StudentStockId",
                table: "StudentStockHistory",
                column: "StudentStockId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentStockHistory_TransactionId",
                table: "StudentStockHistory",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentStocks_StockId",
                table: "StudentStocks",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentStocks_StudentId",
                table: "StudentStocks",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TargetShareId",
                table: "Transactions",
                column: "TargetShareId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RefreshTokens_UserId",
                table: "Users_RefreshTokens",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductGroups");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropTable(
                name: "RolePrivileges");

            migrationBuilder.DropTable(
                name: "ShareTypeInstances");

            migrationBuilder.DropTable(
                name: "StockHistory");

            migrationBuilder.DropTable(
                name: "StudentPurchaseItems");

            migrationBuilder.DropTable(
                name: "Students_RefreshTokens");

            migrationBuilder.DropTable(
                name: "StudentStockHistory");

            migrationBuilder.DropTable(
                name: "Users_RefreshTokens");

            migrationBuilder.DropTable(
                name: "Privileges");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "StudentPurchases");

            migrationBuilder.DropTable(
                name: "StudentStocks");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Stocks");

            migrationBuilder.DropTable(
                name: "Shares");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "ShareTypes");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Instances");
        }
    }
}
