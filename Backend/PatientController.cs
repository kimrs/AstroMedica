using Transport;
using Microsoft.AspNetCore.Mvc;
using Transport.Patient;

namespace Backend;

[ApiController]
[Route("[controller]")]
public class PatientController
{
    private static readonly Dictionary<Id, IPatient> PatientDb = new List<IPatient>()
    {
        new Patient(
            new Id(0),
            new Name("Tony Hoare"),
            ZodiacSign.Aries,
            new PhoneNumber("815 493 00"),
            MailAddress: null
        ),
        new Patient(
            new Id(1),
            new Name("Ada Lovelace"),
            ZodiacSign.Gemini,
            PhoneNumber: null,
            MailAddress: null
        ),
        new Patient(
            new Id(2),
            new Name("Brian Kernighan"),
            ZodiacSign: null,
            PhoneNumber: null,
            new MailAddress("Portveien 2")
        )
    }.ToDictionary(x => x.Id);

    private static Task InitializationTask = Task.Delay(TimeSpan.FromSeconds(5));

    [HttpGet("{idValue}")]
    public IPatient Read(int idValue)
    {
        if (!InitializationTask.IsCompleted)
        {
            return null;
        }

        return PatientDb.TryGetValue(new Id(idValue), out var patient)
            ? patient
            : null;
    }

    [HttpPost]
    public void Create(Patient patient)
    {
        if (PatientDb.ContainsKey(patient.Id))
        {
            return;
        }
        PatientDb.Add(patient.Id, patient);
    }
    
}