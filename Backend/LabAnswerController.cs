using Microsoft.AspNetCore.Mvc;
using Transport;
using Transport.Lab;
using Transport.Patient;

namespace Backend;

[ApiController]
[Route("[controller]")]
public class LabAnswerController
{
    private static readonly Dictionary<Id, List<ILabAnswer>> LabAnswers = new()
    {
        {new Id(0), new List<ILabAnswer>() {new GlucoseLabAnswer(new GlucoseLevel(60))}},
        {new Id(1), new List<ILabAnswer>() {
            new GlucoseLabAnswer(new GlucoseLevel(50)), 
            new Covid19LabAnswer(BinaryLabAnswer.Positive)}},
        {new Id(2), new List<ILabAnswer>() {new GlucoseLabAnswer(new GlucoseLevel(50))}},
        {new Id(3), new List<ILabAnswer>() {new GlucoseLabAnswer(new GlucoseLevel(50))}},
    };

    [HttpGet("{idValue}")]
    public IOption<IEnumerable<ILabAnswer>> Read(int idValue)
    {
        return LabAnswers.TryGetValue(new Id(idValue), out var labAnswers)
            ? new Some<IEnumerable<ILabAnswer>>(labAnswers)
            : new None<IEnumerable<ILabAnswer>>(new ItemDoesNotExist());
    }
}
