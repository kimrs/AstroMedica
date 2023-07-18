// See https://aka.ms/new-console-template for more information

using LabAnswerAnalyser;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Transport;
using Transport.Patient;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var services = new ServiceCollection();
services.AddLogging(c =>
{
    c.AddConsole();
    c.SetMinimumLevel(LogLevel.Information);
});
services.Configure<GlucoseTolerance>(configuration.GetSection("GlucoseTolerance"));
services.AddSingleton(s => s.GetRequiredService<IOptions<GlucoseTolerance>>().Value);

var backendUrl = configuration.GetSection("BackendUrl").Value;
if (backendUrl == null)
{
    throw new ArgumentNullException(nameof(backendUrl));
}

services.AddHttpClient(string.Empty, c =>
{
    c.BaseAddress = new Uri(backendUrl);
});
services.AddSingleton<IPatientService, PatientService>();
services.AddSingleton<ILabAnswerService, LabAnswerService>();
services.AddSingleton<IMailService, MailService>();
services.AddSingleton<ISmsService, SmsService>();
services.AddSingleton<IGlucoseAnalyzer, GlucoseAnalyser>();
    
var patientId = int.Parse(Environment.GetCommandLineArgs()[1]);
var serviceProvider = services.BuildServiceProvider();
using var scope = serviceProvider.CreateScope();
var patientService = scope.ServiceProvider.GetRequiredService<IPatientService>();
if (patientId == 3)
{
    patientService.Create(
        new Patient(
            new Id(3),
            new Name("Grace Hopper"),
            ZodiacSign.Taurus,
            null,
            new MailAddress("Flåklypa")
        ));
}
var glucoseAnalyzer = scope.ServiceProvider.GetRequiredService<IGlucoseAnalyzer>();
    
await glucoseAnalyzer.HandleGlucoseAnalyzedForPatient(patientId);