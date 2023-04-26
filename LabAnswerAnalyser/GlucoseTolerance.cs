using Transport;

namespace LabAnswerAnalyser;

public class GlucoseTolerance
{
    public int DefaultTolerance { get; } = 30;
    public Dictionary<ZodiacSign, int> ZodiacTolerance { get; } = new();
}