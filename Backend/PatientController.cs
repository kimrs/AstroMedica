using Transport;
using Microsoft.AspNetCore.Mvc;
using Transport.Patient;

namespace Backend;

[ApiController]
[Route("[controller]")]
public class PatientController
{
    private static readonly Dictionary<Id, IPatient> _patients = new List<IPatient>()
    {
        new Patient(new Id(0), new Name("Tony Hoare"), ZodiacSign.Aries, new PhoneNumber("815 493 00"), new MailAddressNotSet()),
        new Patient(new Id(1), new Name("Ada Lovelace"), ZodiacSign.Gemini, new PhoneNumberNotSet(), new MailAddressNotSet()),
        new LegacyPatient(new Id(2), new Name("Brian Kernighan"), new PhoneNumberNotSet(), new MailAddress("Portveien 2")),
    }.ToDictionary(x => x.Id);

    private static Task InitializationTask = Task.Delay(TimeSpan.FromSeconds(10));

    [HttpGet]
    [Route("{id}")]
    public IOption Read(int id)
    {
        if (!InitializationTask.IsCompleted)
        {
            return new None(ReasonForNone.ServiceNotYetInitialized);
        }

        return _patients.TryGetValue(new Id(id), out var patient)
            ? new Some<IPatient>(patient)
            : new None(ReasonForNone.PatientDoesNotExist);
    }

    [HttpPost]
    public void Create(Patient patient)
        => _patients.TryAdd(patient.Id, patient);

    [HttpGet]
    public IEnumerable<IPatient> ReadAll()
        => _patients.Values;

    [HttpPut]
    public void Update(Patient patient)
    {
        if (_patients.ContainsKey(patient.Id))
        {
            _patients[patient.Id] = patient;
            return;
        }
        _patients.Add(patient.Id, patient);
    }
    
    [HttpDelete]
    [Route("{id}")]
    public void Delete(Id id)
    {
        _patients.Remove(id);
    }
}