using server.Models.Enums;

namespace server.Models.Constants;

public static class DefaultCategories
{
    public static readonly Dictionary<DefaultCategoryType, (string Name, CategoryType Type, string Color)> Map =
        new()
        {
            { DefaultCategoryType.Groceries, ("Продукты", CategoryType.Expense, "#FF6384") },
            { DefaultCategoryType.Restaurants, ("Кафе и рестораны", CategoryType.Expense, "#FF9F40") },
            { DefaultCategoryType.FastFood, ("Фастфуд", CategoryType.Expense, "#FFCD56") },
            { DefaultCategoryType.Entertainment, ("Развлечения", CategoryType.Expense, "#4BC0C0") },
            { DefaultCategoryType.Housing, ("Проживание", CategoryType.Expense, "#9966FF") },
            { DefaultCategoryType.TransportMetro, ("Проезд(Метро и Автобусы)", CategoryType.Expense, "#36A2EB") },
            { DefaultCategoryType.TransportTaxi, ("Проезд(Каршеринг и такси)", CategoryType.Expense, "#0074D9") },
            { DefaultCategoryType.Household, ("Для дома(Бытовое)", CategoryType.Expense, "#C9CBCF") },
            { DefaultCategoryType.Interior, ("Для дома(Интерьер)", CategoryType.Expense, "#B0BEC5") },
            { DefaultCategoryType.Travel, ("Путешествия", CategoryType.Expense, "#FF6384") },
            { DefaultCategoryType.Subscriptions, ("Связь и подписки", CategoryType.Expense, "#8E44AD") },
            { DefaultCategoryType.Sports, ("Спорт", CategoryType.Expense, "#2ECC71") },
            { DefaultCategoryType.Health, ("Здоровье", CategoryType.Expense, "#E67E22") },
            { DefaultCategoryType.Clothes, ("Одежда", CategoryType.Expense, "#1ABC9C") },
            { DefaultCategoryType.Gift, ("В подарок", CategoryType.Expense, "#D35400") },
            { DefaultCategoryType.Parents, ("Для родителей", CategoryType.Expense, "#5D6D7E") },
            { DefaultCategoryType.OtherExpense, ("Другое", CategoryType.Expense, "#7F8C8D") },

            { DefaultCategoryType.Salary, ("Зарплата", CategoryType.Income, "#27AE60") },
            { DefaultCategoryType.SideJob, ("Подработка", CategoryType.Income, "#2ECC71") },
            { DefaultCategoryType.Cashback, ("Кэшбэк", CategoryType.Income, "#16A085") },
            { DefaultCategoryType.GiftIncome, ("Подарок", CategoryType.Income, "#F39C12") },
            { DefaultCategoryType.OtherIncome, ("Другое", CategoryType.Income, "#BDC3C7") },

        };
}