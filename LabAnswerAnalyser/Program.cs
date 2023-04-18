// See https://aka.ms/new-console-template for more information

using Transport;
using LabAnswerAnalyser;
using Transport.Lab;
using Transport.Patient;

var patientService = new PatientService();
var labAnswerService = new LabAnswerService();
var mailService = new MailService();
var smsService = new SmsService();

var glucoseTolerance = new Dictionary<ZodiacSign, GlucoseLevel>()
{
    { ZodiacSign.Aries,  new GlucoseLevel(30) },
    { ZodiacSign.Gemini, new GlucoseLevel(40) },
    { ZodiacSign.Taurus, new GlucoseLevel(50) }
};
var defaultGlucoseTolerance = new GlucoseLevel(30);

var patientId = new Id(2);
var patientResult = await patientService.Get(patientId);

var patient = patientResult switch
{
    Some<IPatient> s => s.Value,
    None {Because: ReasonForNone.ServiceUnavailable} => throw new Exception(
        "Service unavailable, message placed on the error queue"),
    None {Because: ReasonForNone.ServiceNotYetInitialized} => throw new Exception(
        "Service is still initializing. Retrying in a couple of minutes"),
    None {Because: ReasonForNone.PatientDoesNotExist} => throw new Exception(
        $"Patient with id {patientId} does not exist"),
    _ => throw new Exception($"An error occured"),
};

var labAnswer = labAnswerService.ByPatientId(patient.Id)
    .First(x => x.ExaminationType == ExaminationType.Glucose);

if (labAnswer.GlucoseLevel is null)
{
    return;
}

var shouldNotifyPatient = patient is IHasZodiacSign hasZodiacSign
    ? labAnswer.GlucoseLevel > glucoseTolerance[hasZodiacSign.ZodiacSign]
    : labAnswer.GlucoseLevel > defaultGlucoseTolerance;

if (!shouldNotifyPatient)
{
    return;
}

if (patient.PhoneNumber is PhoneNumber phoneNumber)
{
    smsService.TellPatientToEatLessSugar(phoneNumber, labAnswer);
    return;
}

if (patient.MailAddress is MailAddress mailAddress)
{
    mailService.TellPatientToEatLessSugar(mailAddress, labAnswer);
}





