using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NZWalks.Infrastructure.Migrations.NZWalksAuthDb
{
    /// <inheritdoc />
    public partial class AddAdminRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "48fe7a10-9b65-4402-b37d-e9910c2051d1", "48fe7a10-9b65-4402-b37d-e9910c2051d1", "Admin", "ADMIN" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "48fe7a10-9b65-4402-b37d-e9910c2051d1");
        }
    }
}
