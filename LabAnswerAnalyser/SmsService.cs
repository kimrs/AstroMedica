using Microsoft.Extensions.Logging;
using Transport.Lab;
using Transport.Patient;

namespace LabAnswerAnalyser;

public interface ISmsService
{
    void TellPatientToEatLessSugar(IPhoneNumber phoneNumber, LabAnswer labAnswer);
}

public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;
    public SmsService(
        ILogger<SmsService> logger
    )
    {
        _logger = logger;
    }
    
    public void TellPatientToEatLessSugar(IPhoneNumber phoneNumber, LabAnswer labAnswer)
    {
        _logger.LogInformation(($"Sms was sent to {phoneNumber} to inform about {labAnswer}"));
    }
}