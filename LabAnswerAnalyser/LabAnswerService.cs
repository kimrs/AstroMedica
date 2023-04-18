using Transport;
using Transport.Lab;
using Transport.Patient;

namespace LabAnswerAnalyser;

public class LabAnswerService
{
    public IEnumerable<LabAnswer> ByPatientId(Id patientId)
    {
        return new List<LabAnswer>
        {
            new LabAnswer(ExaminationType.Glucose, null, new GlucoseLevel(50)),
            new LabAnswer(ExaminationType.Covid19, BinaryLabAnswer.Positive, null)
        };
    }
}