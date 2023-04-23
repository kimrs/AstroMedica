namespace Transport.Lab;

public record GlucoseLevel
{
    public int Value { get; }
    public GlucoseLevel(int value)
    {
        Value = value is > 0 and < 100
            ? value
            : throw new ArgumentException($"{nameof(GlucoseLevel)} must be between 0 and 100");
    }

    public static bool operator >(GlucoseLevel a, GlucoseLevel b) => a.Value > b.Value;
    public static bool operator <(GlucoseLevel a, GlucoseLevel b) => a.Value < b.Value;
    public static implicit operator int(GlucoseLevel glucoseLevel) => glucoseLevel.Value;
    public static explicit operator GlucoseLevel(int value) => new(value);
    public override string ToString() => $"{nameof(GlucoseLevel)}:{Value}";
}