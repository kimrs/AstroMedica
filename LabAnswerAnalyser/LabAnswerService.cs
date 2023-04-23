using Newtonsoft.Json;
using Transport;
using Transport.Lab;
using Transport.Patient;

namespace LabAnswerAnalyser;

public interface ILabAnswerService
{
    Task<IOption<IEnumerable<ILabAnswer>>> ByPatientId(Id id);
    Task<IEnumerable<ILabAnswer>> ByPatientThrowIfNone(Id id);
}

public class LabAnswerService : ILabAnswerService
{
    private readonly HttpClient _httpClient;

    public LabAnswerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IOption<IEnumerable<ILabAnswer>>> ByPatientId(Id id)
    {
        HttpResponseMessage result;
        try
        {
            result = await _httpClient.GetAsync($"labanswer/{id.Value}");
        }
        catch (HttpRequestException )
        {
            return new None<IEnumerable<ILabAnswer>>(ReasonForNone.ServiceUnavailable);
        }
        
        var jsonResponse = await result.Content.ReadAsStringAsync();
        var settings = new JsonSerializerSettings();
        settings.TypeNameHandling = TypeNameHandling.All;
        
        return JsonConvert.DeserializeObject<IOption<IEnumerable<ILabAnswer>>>(jsonResponse, settings)
            ?? new None<IEnumerable<ILabAnswer>>(ReasonForNone.FailedToDeserialize);
    }

    public async Task<IEnumerable<ILabAnswer>> ByPatientThrowIfNone(Id id)
    {
        var result = await ByPatientId(id);
        return result.EnsureHasValue();
    }
}