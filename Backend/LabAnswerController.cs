using Microsoft.AspNetCore.Mvc;
using Transport;
using Transport.Lab;
using Transport.Patient;

namespace Backend;

[ApiController]
[Route("[controller]")]
public class LabAnswerController
{
    private static readonly Dictionary<Id, List<LabAnswer>> LabAnswers = new()
    {
        {new Id(0), new List<LabAnswer>() {new LabAnswer(new GlucoseLevel(60), null, ExaminationType.Glucose)}},
        {new Id(1), new List<LabAnswer>() {
            new LabAnswer(new GlucoseLevel(50), null, ExaminationType.Glucose),
            new LabAnswer(null, BinaryLabAnswer.Positive, ExaminationType.Covid19)}},
        {new Id(2), new List<LabAnswer>() {new LabAnswer(new GlucoseLevel(50), null, ExaminationType.Glucose)}},
        {new Id(3), new List<LabAnswer>() {new LabAnswer(new GlucoseLevel(50), null, ExaminationType.Glucose)}},
    };

    [HttpGet("{idValue}")]
    public IEnumerable<LabAnswer> Read(int idValue)
    {
        return LabAnswers.TryGetValue(new Id(idValue), out var labAnswers)
            ? labAnswers
            : null;
    }
}
