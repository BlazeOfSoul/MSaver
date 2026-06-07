using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSaver.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationCurrencyAndCategoryDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationCurrencyCode",
                table: "users",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "BYN");

            migrationBuilder.AddColumn<int>(
                name: "DefaultCategoryType",
                table: "categories",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "categories" existing
                SET "Name" = updates."NewName"
                FROM (
                    VALUES
                        ('Проезд(Метро и Автобусы)', 'Проезд (Метро и Автобусы)', 1, 5),
                        ('Проезд(Каршеринг и такси)', 'Проезд (Каршеринг и такси)', 1, 6),
                        ('Для дома(Бытовое)', 'Для дома (Бытовое)', 1, 7),
                        ('Для дома(Интерьер)', 'Для дома (Интерьер)', 1, 8)
                ) AS updates("OldName", "NewName", "Type", "DefaultType")
                WHERE existing."Name" = updates."OldName"
                  AND existing."Type" = updates."Type"
                  AND NOT EXISTS (
                      SELECT 1
                      FROM "categories" duplicate
                      WHERE duplicate."UserId" = existing."UserId"
                        AND duplicate."Name" = updates."NewName"
                        AND duplicate."Type" = updates."Type"
                  );
                """);

            migrationBuilder.Sql(
                """
                UPDATE "categories" existing
                SET "DefaultCategoryType" = updates."DefaultType"
                FROM (
                    VALUES
                        ('Продукты', 1, 0),
                        ('Кафе и рестораны', 1, 1),
                        ('Фастфуд', 1, 2),
                        ('Развлечения', 1, 3),
                        ('Проживание', 1, 4),
                        ('Проезд (Метро и Автобусы)', 1, 5),
                        ('Проезд(Метро и Автобусы)', 1, 5),
                        ('Проезд (Каршеринг и такси)', 1, 6),
                        ('Проезд(Каршеринг и такси)', 1, 6),
                        ('Для дома (Бытовое)', 1, 7),
                        ('Для дома(Бытовое)', 1, 7),
                        ('Для дома (Интерьер)', 1, 8),
                        ('Для дома(Интерьер)', 1, 8),
                        ('Путешествия', 1, 9),
                        ('Связь и подписки', 1, 10),
                        ('Спорт', 1, 11),
                        ('Здоровье', 1, 12),
                        ('Одежда', 1, 13),
                        ('В подарок', 1, 14),
                        ('Для родителей', 1, 15),
                        ('Другое', 1, 16),
                        ('Зарплата', 2, 17),
                        ('Подработка', 2, 18),
                        ('Кэшбэк', 2, 19),
                        ('Подарок', 2, 20),
                        ('Другое', 2, 21),
                        ('Перевод (поступление)', 3, 998),
                        ('Перевод (выбытие)', 4, 999)
                ) AS updates("Name", "Type", "DefaultType")
                WHERE existing."Name" = updates."Name"
                  AND existing."Type" = updates."Type"
                  AND existing."DefaultCategoryType" IS NULL;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO "categories" (
                    "Id",
                    "UserId",
                    "Name",
                    "Type",
                    "Color",
                    "IsDeleted",
                    "DefaultCategoryType",
                    "CreatedAt",
                    "UpdatedAt"
                )
                SELECT
                    md5(users."Id"::text || ':' || debts."DefaultType"::text)::uuid,
                    users."Id",
                    debts."Name",
                    debts."Type",
                    debts."Color",
                    false,
                    debts."DefaultType",
                    NOW(),
                    NOW()
                FROM "users" users
                CROSS JOIN (
                    VALUES
                        ('Взято в долг (+)', 2, '#38BDF8', 990),
                        ('Возвращено по долгу (-)', 1, '#0EA5E9', 991),
                        ('Дано в долг (-)', 1, '#EC4899', 992),
                        ('Отдано по долгу (+)', 2, '#A3E635', 993)
                ) AS debts("Name", "Type", "Color", "DefaultType")
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "categories" existing
                    WHERE existing."UserId" = users."Id"
                      AND (
                          existing."DefaultCategoryType" = debts."DefaultType"
                          OR (existing."Name" = debts."Name" AND existing."Type" = debts."Type")
                      )
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationCurrencyCode",
                table: "users");

            migrationBuilder.DropColumn(
                name: "DefaultCategoryType",
                table: "categories");
        }
    }
}
