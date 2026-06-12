using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSaver.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HardenBackendOperationalGuards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tags_UserId_Name",
                table: "tags");

            migrationBuilder.DropIndex(
                name: "IX_categories_UserId_Name_Type",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "IX_accounts_UserId_Name",
                table: "accounts");

            migrationBuilder.CreateIndex(
                name: "IX_tags_UserId_Name",
                table: "tags",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_UserId_Name",
                table: "categories",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_accounts_UserId_Name",
                table: "accounts",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "\"IsArchived\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tags_UserId_Name",
                table: "tags");

            migrationBuilder.DropIndex(
                name: "IX_categories_UserId_Name",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "IX_accounts_UserId_Name",
                table: "accounts");

            migrationBuilder.CreateIndex(
                name: "IX_tags_UserId_Name",
                table: "tags",
                columns: new[] { "UserId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_categories_UserId_Name_Type",
                table: "categories",
                columns: new[] { "UserId", "Name", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_UserId_Name",
                table: "accounts",
                columns: new[] { "UserId", "Name" });
        }
    }
}
