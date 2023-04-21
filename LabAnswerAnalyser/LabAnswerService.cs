using Transport.Lab;
using Transport.Patient;

namespace LabAnswerAnalyser;

public class LabAnswerService
{
    public IEnumerable<ILabAnswer> ByPatientId(Id _)
    {
        return new List<ILabAnswer>
        {
            new GlucoseLabAnswer(new GlucoseLevel(50)),
            new Covid19LabAnswer(BinaryLabAnswer.Negative)
        };
    }
}