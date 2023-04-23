using Transport;
using Microsoft.AspNetCore.Mvc;
using Transport.Patient;

namespace Backend;

[ApiController]
[Route("[controller]")]
public class PatientController
{
    private static readonly Dictionary<Id, IPatient> Patients = new List<IPatient>()
    {
        new Patient(new Id(0), new Name("Tony Hoare"), ZodiacSign.Aries, new PhoneNumber("815 493 00"), new MailAddressNotSet()),
        new Patient(new Id(1), new Name("Ada Lovelace"), ZodiacSign.Gemini, new PhoneNumberNotSet(), new MailAddressNotSet()),
        new LegacyPatient(new Id(2), new Name("Brian Kernighan"), new PhoneNumberNotSet(), new MailAddress("Portveien 2")),
    }.ToDictionary(x => x.Id);

    private static Task InitializationTask = Task.Delay(TimeSpan.FromSeconds(5));

    private readonly ILogger<PatientController> _logger;
    public PatientController(ILogger<PatientController> logger)
    {
        _logger = logger;
    }

    [HttpGet("{idValue}")]
    public IOption<IPatient> Read(int idValue)
    {
        if (!InitializationTask.IsCompleted)
        {
            return new None<IPatient>(ReasonForNone.ServiceNotYetInitialized);
        }
        
        return Patients.TryGetValue(new Id(idValue), out var patient)
            ? new Some<IPatient>(patient)
            : new None<IPatient>(ReasonForNone.ItemDoesNotExist);
    }

    [HttpPost]
    public void Create(Patient patient)
    {
        if (Patients.ContainsKey(patient.Id))
        {
            return;
        }
        Patients.Add(patient.Id, patient);
    }
    
}