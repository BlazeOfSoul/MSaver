namespace MSaver.Domain.Common;

public static class MoneyPrecision
{
    public static bool Fits(decimal amount, short precision) =>
        Math.Round(amount, precision, MidpointRounding.AwayFromZero) == amount;
}
