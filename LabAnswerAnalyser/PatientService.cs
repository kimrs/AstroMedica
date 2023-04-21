using Newtonsoft.Json;
using Transport;
using Transport.Patient;

namespace LabAnswerAnalyser;

public class PatientService
{
    private static readonly HttpClient _httpClient = new ()
    {
        BaseAddress = new Uri("http://localhost:5098/"),
    };

    public async Task<IOption> Get(Id id)
    {
        var op = new JsonSerializerSettings() {TypeNameHandling = TypeNameHandling.All};
        try
        {
            var result = await _httpClient.GetAsync($"patient/{id.Value}");
            var jsonResponse = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IOption>(jsonResponse, op);
        }
        catch (HttpRequestException)
        {
            return new None(ReasonForNone.ServiceUnavailable);
        }
    }
}