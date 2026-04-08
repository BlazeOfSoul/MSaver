namespace MSaver.Domain.Entities;

public sealed class TransactionTag : Entity
{
    private TransactionTag() { }

    public Guid TransactionId { get; private set; }
    public Transaction? Transaction { get; private set; }

    public Guid TagId { get; private set; }
    public Tag? Tag { get; private set; }

    public static TransactionTag Create(Guid transactionId, Guid tagId)
    {
        return new TransactionTag
        {
            TransactionId = transactionId,
            TagId = tagId
        };
    }
}