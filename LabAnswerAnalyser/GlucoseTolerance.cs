using Transport;

namespace LabAnswerAnalyser;

public class GlucoseTolerance
{
    public int DefaultTolerance { get; }
    public Dictionary<ZodiacSign, int> ZodiacTolerance { get; } = new();
}