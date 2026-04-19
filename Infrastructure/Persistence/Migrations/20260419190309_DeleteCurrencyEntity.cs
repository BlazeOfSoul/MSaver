using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSaver.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DeleteCurrencyEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_accounts_currencies_CurrencyId",
                table: "accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_categories_categories_ParentId",
                table: "categories");

            migrationBuilder.DropForeignKey(
                name: "FK_transactions_currencies_CurrencyId",
                table: "transactions");

            migrationBuilder.DropTable(
                name: "currencies");

            migrationBuilder.DropIndex(
                name: "IX_transactions_CurrencyId",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "IX_categories_ParentId",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "IX_accounts_CurrencyId",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "accounts");

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "accounts",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "accounts");

            migrationBuilder.AddColumn<Guid>(
                name: "CurrencyId",
                table: "transactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CurrencyId",
                table: "accounts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "currencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Precision = table.Column<short>(type: "smallint", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currencies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_transactions_CurrencyId",
                table: "transactions",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_categories_ParentId",
                table: "categories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_accounts_CurrencyId",
                table: "accounts",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_currencies_Code",
                table: "currencies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_currencies_IsDefault",
                table: "currencies",
                column: "IsDefault");

            migrationBuilder.AddForeignKey(
                name: "FK_accounts_currencies_CurrencyId",
                table: "accounts",
                column: "CurrencyId",
                principalTable: "currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_categories_categories_ParentId",
                table: "categories",
                column: "ParentId",
                principalTable: "categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_transactions_currencies_CurrencyId",
                table: "transactions",
                column: "CurrencyId",
                principalTable: "currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
