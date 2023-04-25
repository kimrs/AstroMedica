namespace Transport.Lab;

public record LabAnswer(GlucoseLevel GlucoseLevel, BinaryLabAnswer? BinaryLabAnswer, ExaminationType ExaminationType)
{
    public static bool operator >(LabAnswer a, GlucoseLevel b) => a.GlucoseLevel > b;
    public static bool operator <(LabAnswer a, GlucoseLevel b) => a.GlucoseLevel < b;

    public override string ToString() => ExaminationType switch
    {
        ExaminationType.Glucose => $"{nameof(LabAnswer)}:{GlucoseLevel}",
        ExaminationType.Covid19 => $"{nameof(LabAnswer)}:{BinaryLabAnswer}",
        _ => string.Empty
    };
}