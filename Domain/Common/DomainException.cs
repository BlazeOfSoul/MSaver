namespace MSaver.Domain.Common;

public sealed class DomainException : Exception
{
    public DomainError Error
    {
        get;
    }

    public DomainException(DomainError error)
        : base(error.Message)
    {
        Error = error;
    }
}
