using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSaver.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_transactions_AccountId_Date",
                table: "transactions",
                columns: new[] { "AccountId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_transactions_UserId_CategoryId_Date",
                table: "transactions",
                columns: new[] { "UserId", "CategoryId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_transactions_UserId_Date_Id",
                table: "transactions",
                columns: new[] { "UserId", "Date", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_transactions_AccountId_Date",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "IX_transactions_UserId_CategoryId_Date",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "IX_transactions_UserId_Date_Id",
                table: "transactions");
        }
    }
}
