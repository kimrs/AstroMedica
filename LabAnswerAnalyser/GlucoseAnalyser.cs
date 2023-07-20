using Microsoft.Extensions.Logging;
using Transport;
using Transport.Lab;
using Transport.Patient;

namespace LabAnswerAnalyser;

public interface IGlucoseAnalyzer
{
    Task HandleGlucoseAnalyzedForPatient(Id patientId);
}

public class GlucoseAnalyser : IGlucoseAnalyzer
{
    private readonly IPatientService _patientService;
    private readonly ILabAnswerService _labAnswerService;
    private readonly GlucoseTolerance _glucoseTolerance;
    private readonly ISmsService _smsService;
    private readonly IMailService _mailService;
    private readonly ILogger<GlucoseAnalyser> _logger;

    public GlucoseAnalyser(
        IPatientService patientService,
        ILabAnswerService labAnswerService,
        GlucoseTolerance glucoseTolerance,
        ISmsService smsService,
        IMailService mailService,
        ILogger<GlucoseAnalyser> logger
    )
    {
        _patientService = patientService;
        _labAnswerService = labAnswerService;
        _glucoseTolerance = glucoseTolerance;
        _smsService = smsService;
        _mailService = mailService;
        _logger = logger;
    }
    
    public async Task HandleGlucoseAnalyzedForPatient(Id patientId)
    {
        var maybePatient = await _patientService.Get(patientId);
        if (maybePatient is None<IPatient> {Because: ItemDoesNotExist or ServiceNotYetInitialized})
        {
            _logger.LogInformation("Trying again");
            
            await Task.Delay(TimeSpan.FromSeconds(10));
            await HandleGlucoseAnalyzedForPatient(patientId);
            return;
        }

        var patient = maybePatient.EnsureHasValue();
        
        var labAnswers = await _labAnswerService.ByPatientThrowIfNone(patient.Id);
        var labAnswer = labAnswers.OfType<GlucoseLabAnswer>().First();
        
        var shouldNotifyPatient = patient is IHasZodiacSign hasZodiacSign
            ? labAnswer.GlucoseLevel > _glucoseTolerance.ZodiacTolerance[hasZodiacSign.ZodiacSign]
            : labAnswer.GlucoseLevel > _glucoseTolerance.DefaultTolerance;
            
        if (!shouldNotifyPatient)
        {
            return;
        }
        
        if (patient.PhoneNumber is not PhoneNumberNotSet)
        {
            _smsService.TellPatientToEatLessSugar(patient.PhoneNumber, labAnswer);
            return;
        }
            
        if (patient.MailAddress is not MailAddressNotSet)
        {
            _mailService.TellPatientToEatLessSugar(patient.MailAddress, labAnswer);
        }
    }
}