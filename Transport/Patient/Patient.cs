namespace Transport.Patient;

public interface IPatient
{
    Id Id { get; }
    string Name { get; }
    IPhoneNumber PhoneNumber { get; }
    IMailAddress MailAddress { get; }
}

public interface IPhoneNumber {}

public record PhoneNumberNotSet : IPhoneNumber;

public record PhoneNumber(string Value) : IPhoneNumber;

public interface IMailAddress { }

public record MailAddressNotSet : IMailAddress;

public record MailAddress(string Value) : IMailAddress;

public interface IHasZodiacSign
{
    ZodiacSign ZodiacSign { get; }
}

public record Patient(
    Id Id,
    string Name,
    ZodiacSign ZodiacSign,
    IPhoneNumber PhoneNumber,
    IMailAddress MailAddress
) : IPatient, IHasZodiacSign;

public record LegacyPatient(
    Id Id,
    string Name,
    IPhoneNumber PhoneNumber,
    IMailAddress MailAddress
) : IPatient;