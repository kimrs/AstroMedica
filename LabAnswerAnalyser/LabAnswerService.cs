using Newtonsoft.Json;
using Transport.Lab;
using Transport.Patient;

namespace LabAnswerAnalyser;

public interface ILabAnswerService
{
    Task<IEnumerable<LabAnswer>> ByPatientId(Id id);
    Task<IEnumerable<LabAnswer>> ByPatientThrowIfNone(Id id);
}

public class LabAnswerService : ILabAnswerService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerSettings _settings = new() {TypeNameHandling = TypeNameHandling.All};

    public LabAnswerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<LabAnswer>> ByPatientId(Id id)
    {
        HttpResponseMessage result;
        try
        {
            result = await _httpClient.GetAsync($"labanswer/{id.Value}");
        }
        catch (HttpRequestException )
        {
            return null;
        }
        
        var jsonResponse = await result.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<IEnumerable<LabAnswer>>(jsonResponse, _settings);
    }

    public async Task<IEnumerable<LabAnswer>> ByPatientThrowIfNone(Id id)
    {
        var result = await ByPatientId(id);
        if (result is null)
        {
            throw new Exception("LabAnswer is null");
        }

        return result;
    }
}