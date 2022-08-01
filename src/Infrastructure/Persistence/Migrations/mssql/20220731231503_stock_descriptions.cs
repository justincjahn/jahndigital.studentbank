using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JahnDigital.StudentBank.Infrastructure.Persistence.Migrations.mssql
{
    public partial class stock_descriptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableShares",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "TotalShares",
                table: "Stocks");

            migrationBuilder.AddColumn<string>(
                name: "FormattedDescription",
                table: "Stocks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RawDescription",
                table: "Stocks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                "UPDATE Stocks SET RawDescription = '', FormattedDescription = ''"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormattedDescription",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "RawDescription",
                table: "Stocks");

            migrationBuilder.AddColumn<long>(
                name: "AvailableShares",
                table: "Stocks",
                type: "bigint",
                nullable: false,
                defaultValue: 1000000L);

            migrationBuilder.AddColumn<long>(
                name: "TotalShares",
                table: "Stocks",
                type: "bigint",
                nullable: false,
                defaultValue: 1000000L);
        }
    }
}
