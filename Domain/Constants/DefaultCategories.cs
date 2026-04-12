using MSaver.Domain.Enums;

namespace MSaver.Domain.Constants;

public static class DefaultCategories
{
    public static readonly IReadOnlyDictionary<DefaultCategoryType, (string Name, CategoryType Type, string Color)> Map =
        new Dictionary<DefaultCategoryType, (string, CategoryType, string)>
        {
            { DefaultCategoryType.Groceries, ("Продукты", CategoryType.Debit, "#FF6384") },
            { DefaultCategoryType.Restaurants, ("Кафе и рестораны", CategoryType.Debit, "#FF9F40") },
            { DefaultCategoryType.FastFood, ("Фастфуд", CategoryType.Debit, "#FFCD56") },
            { DefaultCategoryType.Entertainment, ("Развлечения", CategoryType.Debit, "#4BC0C0") },
            { DefaultCategoryType.Housing, ("Проживание", CategoryType.Debit, "#9966FF") },
            { DefaultCategoryType.TransportMetro, ("Проезд(Метро и Автобусы)", CategoryType.Debit, "#36A2EB") },
            { DefaultCategoryType.TransportTaxi, ("Проезд(Каршеринг и такси)", CategoryType.Debit, "#0074D9") },
            { DefaultCategoryType.Household, ("Для дома(Бытовое)", CategoryType.Debit, "#C9CBCF") },
            { DefaultCategoryType.Interior, ("Для дома(Интерьер)", CategoryType.Debit, "#B0BEC5") },
            { DefaultCategoryType.Travel, ("Путешествия", CategoryType.Debit, "#FF6384") },
            { DefaultCategoryType.Subscriptions, ("Связь и подписки", CategoryType.Debit, "#8E44AD") },
            { DefaultCategoryType.Sports, ("Спорт", CategoryType.Debit, "#2ECC71") },
            { DefaultCategoryType.Health, ("Здоровье", CategoryType.Debit, "#E67E22") },
            { DefaultCategoryType.Clothes, ("Одежда", CategoryType.Debit, "#1ABC9C") },
            { DefaultCategoryType.Gift, ("В подарок", CategoryType.Debit, "#D35400") },
            { DefaultCategoryType.Parents, ("Для родителей", CategoryType.Debit, "#5D6D7E") },
            { DefaultCategoryType.OtherExpense, ("Другое", CategoryType.Debit, "#7F8C8D") },

            { DefaultCategoryType.Salary, ("Зарплата", CategoryType.Credit, "#27AE60") },
            { DefaultCategoryType.SideJob, ("Подработка", CategoryType.Credit, "#2ECC71") },
            { DefaultCategoryType.Cashback, ("Кэшбэк", CategoryType.Credit, "#16A085") },
            { DefaultCategoryType.GiftIncome, ("Подарок", CategoryType.Credit, "#F39C12") },
            { DefaultCategoryType.OtherIncome, ("Другое", CategoryType.Credit, "#BDC3C7") },
        };
}