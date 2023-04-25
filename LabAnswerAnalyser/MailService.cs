using Microsoft.Extensions.Logging;
using Transport.Lab;
using Transport.Patient;

namespace LabAnswerAnalyser;

public interface IMailService
{
    void TellPatientToEatLessSugar(MailAddress mailAddress, LabAnswer labAnswer);
}

public class MailService : IMailService
{
    private readonly ILogger<MailService> _logger;
    public MailService(
        ILogger<MailService> logger
        )
    {
        _logger = logger;
    }
    
    public void TellPatientToEatLessSugar(MailAddress mailAddress, LabAnswer labAnswer)
    {
        _logger.LogInformation($"Letter was sent to {mailAddress} to inform about {labAnswer}");
    }
}