namespace Transport.Patient;

public interface IPatient
{
    Id Id { get; }
    Name Name { get; }
    IPhoneNumber PhoneNumber { get; }
    IMailAddress MailAddress { get; }
}

public interface IHasZodiacSign
{
    ZodiacSign ZodiacSign { get; }
}

public record Patient(
    Id Id,
    Name Name,
    ZodiacSign ZodiacSign,
    IPhoneNumber PhoneNumber,
    IMailAddress MailAddress
) : IPatient, IHasZodiacSign;

public record LegacyPatient(
    Id Id,
    Name Name,
    IPhoneNumber PhoneNumber,
    IMailAddress MailAddress
) : IPatient;

public interface IPhoneNumber { }

public record PhoneNumberNotSet : IPhoneNumber;

public record PhoneNumber(string Value)
    : IPhoneNumber
{
    public override string ToString() => Value;
}

public interface IMailAddress { }

public record MailAddressNotSet : IMailAddress;

public record MailAddress(string Value)
    : IMailAddress
{
    public override string ToString() => Value;
}
