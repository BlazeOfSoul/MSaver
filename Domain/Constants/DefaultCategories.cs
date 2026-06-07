using MSaver.Domain.Enums;

namespace MSaver.Domain.Constants;

public static class DefaultCategories
{
    public static readonly IReadOnlyDictionary<DefaultCategoryType, (string Name, CategoryType Type, string Color)> Map =
        new Dictionary<DefaultCategoryType, (string, CategoryType, string)>
        {
            { DefaultCategoryType.Groceries, ("Продукты", CategoryType.Debit, "#F97373") },
            { DefaultCategoryType.Restaurants, ("Кафе и рестораны", CategoryType.Debit, "#FB923C") },
            { DefaultCategoryType.FastFood, ("Фастфуд", CategoryType.Debit, "#F59E0B") },
            { DefaultCategoryType.Entertainment, ("Развлечения", CategoryType.Debit, "#A78BFA") },
            { DefaultCategoryType.Housing, ("Проживание", CategoryType.Debit, "#60A5FA") },
            { DefaultCategoryType.TransportMetro, ("Проезд (Метро и Автобусы)", CategoryType.Debit, "#22D3EE") },
            { DefaultCategoryType.TransportTaxi, ("Проезд (Каршеринг и такси)", CategoryType.Debit, "#0EA5E9") },
            { DefaultCategoryType.Household, ("Для дома (Бытовое)", CategoryType.Debit, "#38BDF8") },
            { DefaultCategoryType.Interior, ("Для дома (Интерьер)", CategoryType.Debit, "#818CF8") },
            { DefaultCategoryType.Travel, ("Путешествия", CategoryType.Debit, "#C084FC") },
            { DefaultCategoryType.Subscriptions, ("Связь и подписки", CategoryType.Debit, "#8B5CF6") },
            { DefaultCategoryType.Sports, ("Спорт", CategoryType.Debit, "#34D399") },
            { DefaultCategoryType.Health, ("Здоровье", CategoryType.Debit, "#10B981") },
            { DefaultCategoryType.Clothes, ("Одежда", CategoryType.Debit, "#14B8A6") },
            { DefaultCategoryType.Gift, ("В подарок", CategoryType.Debit, "#F472B6") },
            { DefaultCategoryType.Parents, ("Для родителей", CategoryType.Debit, "#FBBF24") },
            { DefaultCategoryType.OtherExpense, ("Другое", CategoryType.Debit, "#94A3B8") },

            { DefaultCategoryType.Salary, ("Зарплата", CategoryType.Credit, "#22C55E") },
            { DefaultCategoryType.SideJob, ("Подработка", CategoryType.Credit, "#16A34A") },
            { DefaultCategoryType.Cashback, ("Кэшбэк", CategoryType.Credit, "#2DD4BF") },
            { DefaultCategoryType.GiftIncome, ("Подарок", CategoryType.Credit, "#F59E0B") },
            { DefaultCategoryType.OtherIncome, ("Другое", CategoryType.Credit, "#A3E635") },

            { DefaultCategoryType.DebtTaken, ("Взято в долг (+)", CategoryType.Credit, "#38BDF8") },
            { DefaultCategoryType.DebtReturned, ("Возвращено по долгу (-)", CategoryType.Debit, "#0EA5E9") },
            { DefaultCategoryType.DebtGiven, ("Дано в долг (-)", CategoryType.Debit, "#EC4899") },
            { DefaultCategoryType.DebtPaidBack, ("Отдано по долгу (+)", CategoryType.Credit, "#A3E635") },

            { DefaultCategoryType.TransferIncome, ("Перевод (поступление)", CategoryType.TransferIncome, "#64748B") },
            { DefaultCategoryType.TransferExpense, ("Перевод (выбытие)", CategoryType.TransferExpense, "#64748B") },
        };
}
