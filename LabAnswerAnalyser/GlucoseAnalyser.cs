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

    /* For debugging purposes */
    private const int PatientWithPhoneNumber = 0;
    private const int PatientWithCovid = 1;
    private const int LegacyPatient = 2;
    private const int LazyInitializedPatient = 3;
    public const int PatientId = LazyInitializedPatient;
    /* For debugging purposes */
    
    public async Task HandleGlucoseAnalyzedForPatient(Id patientId)
    {
        var maybePatient = await _patientService.Get(patientId);
        if (maybePatient is None<IPatient> {Because: ItemDoesNotExist or ServiceNotYetInitialized} none)
        {
            _logger.LogInformation("Trying again because {none}", none);
            await Task.Delay(TimeSpan.FromSeconds(10));
            await HandleGlucoseAnalyzedForPatient(patientId);
            return;
        }
        var patient = maybePatient.EnsureHasValue();
        
        var labAnswers = await _labAnswerService.ByPatientThrowIfNone(patient.Id);
        
        var labAnswer = labAnswers.OfType<GlucoseLabAnswer>().First();
        var shouldNotifyPatient = patient is IHasZodiacSign hasZodiacSign
            ? labAnswer.Value > _glucoseTolerance.ZodiacTolerance[hasZodiacSign.ZodiacSign]
            : labAnswer.Value > _glucoseTolerance.DefaultTolerance;
            
        if (!shouldNotifyPatient)
        {
            return;
        }
            
        if (patient.PhoneNumber is PhoneNumber phoneNumber)
        {
            _smsService.TellPatientToEatLessSugar(phoneNumber, labAnswer);
            return;
        }
            
        if (patient.MailAddress is MailAddress mailAddress)
        {
            _mailService.TellPatientToEatLessSugar(mailAddress, labAnswer);
        }
    }
}