namespace Transport.Lab;

public interface ILabAnswer
{
}

public record GlucoseLabAnswer(GlucoseLevel Value) : ILabAnswer
{
    public static bool operator >(GlucoseLabAnswer a, GlucoseLevel b) => a.Value > b;
    public static bool operator <(GlucoseLabAnswer a, GlucoseLevel b) => a.Value < b;
}

public record Covid19LabAnswer(BinaryLabAnswer Value) : ILabAnswer;