namespace Domain.ValueObjects;

public readonly record struct PaymentId(Guid Value)
{
    public static implicit operator Guid(PaymentId id) => id.Value;
    public static implicit operator PaymentId(Guid value) => new(value);
}