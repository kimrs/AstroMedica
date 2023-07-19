namespace Transport.Lab;

public interface ILabAnswer { }

public record GlucoseLabAnswer(GlucoseLevel GlucoseLevel)
    : ILabAnswer
{
    public static bool operator >(GlucoseLabAnswer a, GlucoseLevel b) => a.GlucoseLevel > b;
    public static bool operator <(GlucoseLabAnswer a, GlucoseLevel b) => a.GlucoseLevel < b;
    public override string ToString() => $"{nameof(GlucoseLabAnswer)}:{GlucoseLevel}";
}

public record Covid19LabAnswer(BinaryLabAnswer BinaryLabAnswer)
    : ILabAnswer
{
    public override string ToString() => $"{nameof(Covid19LabAnswer)}:{BinaryLabAnswer}";
}

