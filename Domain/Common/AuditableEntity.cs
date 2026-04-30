namespace MSaver.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAt { get; protected set; }

    public DateTime UpdatedAt { get; protected set; }
}