using Microsoft.AspNetCore.Mvc;
using Transport;
using Transport.Lab;
using Transport.Patient;

namespace Backend;

[ApiController]
[Route("[controller]")]
public class LabAnswerController
{
    private static readonly Dictionary<Id, List<LabAnswer>> LabAnswerDb = new()
    {
        {
            new Id(0),
            new List<LabAnswer>
            {
                new (
                    new GlucoseLevel(60),
                    BinaryLabAnswer: null,
                    ExaminationType.Glucose
                )
            }
        },
        {
            new Id(1),
            new List<LabAnswer>
            {
                new (
                    new GlucoseLevel(50),
                    BinaryLabAnswer: null,
                    ExaminationType.Glucose
                ),
                new (
                    GlucoseLevel: null,
                    BinaryLabAnswer.Positive,
                    ExaminationType.Covid19
                )
            }
        },
        {
            new Id(2),
            new List<LabAnswer>
            {
                new (
                    new GlucoseLevel(50),
                    BinaryLabAnswer: null,
                    ExaminationType.Glucose
                )
            }
        },
        {
            new Id(3),
            new List<LabAnswer>
            {
                new (
                    new GlucoseLevel(50),
                    BinaryLabAnswer: null,
                    ExaminationType.Glucose
                )
            }
        }
    };

    [HttpGet("{idValue}")]
    public IEnumerable<LabAnswer> Read(int idValue)
    {
        return LabAnswerDb.TryGetValue(new Id(idValue), out var labAnswers)
            ? labAnswers
            : null;
    }
}
