using Transport.Lab;
using Transport.Patient;

namespace LabAnswerAnalyser;

public class SmsService
{
    public void TellPatientToEatLessSugar(PhoneNumber phoneNumber, ILabAnswer labAnswer)
    {
        Console.Out.WriteLine($"Sms was sent to {phoneNumber} to notify about {labAnswer}");
    }
}