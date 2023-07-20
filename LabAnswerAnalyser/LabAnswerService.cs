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
    private readonly JsonSerializerSettings _settings = new() {TypeNameHandling = TypeNameHandling.All};

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
            return new None<IEnumerable<ILabAnswer>>(new ServiceUnavailable());
        }
        
        var jsonResponse = await result.Content.ReadAsStringAsync();

        try
        {
            return JsonConvert.DeserializeObject<IOption<IEnumerable<ILabAnswer>>>(jsonResponse, _settings)
                   ?? new None<IEnumerable<ILabAnswer>>(new FailedToDeserialize());
        }
        catch (JsonReaderException)
        {
            return new None<IEnumerable<ILabAnswer>>(new FailedToDeserialize());
        }
    }

    public async Task<IEnumerable<ILabAnswer>> ByPatientThrowIfNone(Id id)
    {
        var result = await ByPatientId(id);

        return result.EnsureHasValue();
    }
}