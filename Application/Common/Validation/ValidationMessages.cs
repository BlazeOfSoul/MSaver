namespace MSaver.Application.Common.Validation;

public static class ValidationMessages
{
    public const string Required = "Поле обязательно для заполнения.";
    public const string EmailInvalid = "Некорректный email.";
    public const string MaxLength = "Длина поля не должна превышать {MaxLength} символов.";
    public const string MinLength = "Длина поля должна быть не менее {MinLength} символов.";
    public const string MustBePositive = "Значение должно быть больше нуля.";
}