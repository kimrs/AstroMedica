using Transport;
using Transport.Lab;
using Transport.Patient;

namespace LabAnswerAnalyser;

public class MailService
{
    public void TellPatientToEatLessSugar(MailAddress mailAddress, ILabAnswer labAnswer)
    {
        Console.Out.WriteLine($"Mail was sent to {mailAddress} to notify about {labAnswer}");
    }
}