namespace Transport.Lab;

public record GlucoseLevel
{
    private int _value;
    public GlucoseLevel(int value)
    {
        _value = value is > 0 and < 100
            ? value
            : throw new ArgumentException($"{nameof(GlucoseLevel)} must be between 0 and 100");
    }

    public static bool operator >(GlucoseLevel a, GlucoseLevel b) => a._value > b._value;
    public static bool operator <(GlucoseLevel a, GlucoseLevel b) => a._value < b._value;
}