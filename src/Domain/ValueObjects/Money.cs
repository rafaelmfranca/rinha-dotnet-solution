namespace Domain.ValueObjects;

public readonly record struct Money(decimal Amount)
{
    public static implicit operator decimal(Money money) => money.Amount;
    public static implicit operator Money(decimal amount) => new(amount);
}