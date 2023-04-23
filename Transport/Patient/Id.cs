namespace Transport.Patient;

public record Id(int Value)
{
    public static implicit operator int(Id id) => id.Value;
    public static implicit operator Id(int value) => new (value);
    public override string ToString() => Value.ToString();
}