using Transport;
using Microsoft.AspNetCore.Mvc;
using Transport.Patient;

namespace Backend;

[ApiController]
[Route("[controller]")]
public class PatientController
{
    private static readonly Dictionary<Id, Patient> Patients = new List<Patient>()
    {
        new Patient(new Id(0), new Name("Tony Hoare"), ZodiacSign.Aries, new PhoneNumber("815 493 00"), null),
        new Patient(new Id(1), new Name("Ada Lovelace"), ZodiacSign.Gemini, null, null),
        new Patient(new Id(2), new Name("Brian Kernighan"), null, null, new MailAddress("Portveien 2")),
    }.ToDictionary(x => x.Id);

    private static readonly Task InitializationTask = Task.Delay(TimeSpan.FromSeconds(5));

    private readonly ILogger<PatientController> _logger;
    public PatientController(ILogger<PatientController> logger)
    {
        _logger = logger;
    }

    [HttpGet("{idValue}")]
    public IOption<Patient> Read(int idValue)
    {
        if (!InitializationTask.IsCompleted)
        {
            return new None<Patient>(ReasonForNone.ServiceNotYetInitialized);
        }

        return Patients.TryGetValue(new Id(idValue), out var patient)
            ? new Some<Patient>(patient)
            : new None<Patient>(ReasonForNone.ItemDoesNotExist);
    }

    [HttpPost]
    public void Create(Patient patient)
    {
        if (Patients.ContainsKey(patient.Id))
        {
            return;
        }
        Patients.Add(patient.Id, patient);
        _logger.LogInformation("Created {patient}", patient);
    }
    
}