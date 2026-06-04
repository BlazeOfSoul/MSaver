namespace MSaver.Application.Common.Models;

public static class DateTimeUtc
{
    public static DateTime Normalize(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    public static DateTime? Normalize(DateTime? value)
    {
        return value.HasValue
            ? Normalize(value.Value)
            : null;
    }
}
