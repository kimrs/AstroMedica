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
        new LegacyPatient(
            new Id(2),
            new Name("Brian Kernighan"),
            PhoneNumber: null,
            new MailAddress("Portveien 2")
        )
    }.ToDictionary(x => x.Id);

    private static Task _initializationTask = Task.Delay(TimeSpan.FromSeconds(5));

    [HttpGet("{idValue}")]
    public IOption<IPatient> Read(int idValue)
    {
        if (!_initializationTask.IsCompleted)
        {
            return new None<IPatient>(new ServiceNotYetInitialized());
        }

        return PatientDb.TryGetValue(new Id(idValue), out var patient)
            ? new Some<IPatient>(patient)
            : new None<IPatient>(new ItemDoesNotExist());
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