using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;
using Transport;
using Transport.Patient;

namespace LabAnswerAnalyser;

public interface IPatientService
{
    Task<IOption<IPatient>> Get(Id id);
    Task Create(Patient patient);
}

public class PatientService : IPatientService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerSettings _settings = new() {TypeNameHandling = TypeNameHandling.All};

    public PatientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IOption<IPatient>> Get(Id id)
    {
        HttpResponseMessage result;
        try
        {
            result = await _httpClient.GetAsync($"patient/{id.Value}");
        }
        catch (HttpRequestException)
        {
            return new None<IPatient>(ReasonForNone.ServiceUnavailable);
        }
        var jsonResponse = await result.Content.ReadAsStringAsync();

        try
        {
            return JsonConvert.DeserializeObject<IOption<IPatient>>(jsonResponse, _settings)
                   ?? new None<IPatient>(ReasonForNone.FailedToDeserialize);
        }
        catch (JsonReaderException)
        {
            return new None<IPatient>(ReasonForNone.FailedToDeserialize);
        }
    }

    public async Task Create(Patient patient)
    {
        // await Task.Delay(TimeSpan.FromSeconds(3));
        var settings = new JsonSerializerSettings();
        settings.TypeNameHandling = TypeNameHandling.All;
        var json = JsonConvert.SerializeObject(patient, settings);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        await _httpClient.PostAsync("patient", httpContent);
    }
}