namespace Transport.Lab;

public record LabAnswer(ExaminationType ExaminationType, BinaryLabAnswer? BinaryLabAnswer, GlucoseLevel? GlucoseLevel);