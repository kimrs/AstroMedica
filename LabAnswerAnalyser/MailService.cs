using Transport;
using Transport.Lab;
using Transport.Patient;

namespace LabAnswerAnalyser;

public class MailService
{
    public void TellPatientToEatLessSugar(MailAddress mailAddress, LabAnswer labAnswer)
    {
        Console.Out.WriteLine($"Mail was sent to {mailAddress} to notify about {labAnswer.ExaminationType}");
    }
}