namespace MSaver.Application.Common.Validation;

public static class ValidationMessages
{
    public const string Required = "Поле обязательно для заполнения.";
    public const string EmailInvalid = "Некорректный email.";
    public const string MaxLength = "Длина поля не должна превышать {MaxLength} символов.";
    public const string MinLength = "Длина поля должна быть не менее {MinLength} символов.";
    public const string MustBePositive = "Значение должно быть больше нуля.";
    public const string MustBeZeroOrPositive = "Значение должно быть больше или равно нулю.";
    public const string MustNotBeZero = "Значение не должно быть равно нулю.";
    public const string InvalidId = "Некорректный идентификатор.";
    public const string InvalidDate = "Некорректная дата.";
    public const string CollectionRequired = "Необходимо указать хотя бы одно значение.";
}