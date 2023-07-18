namespace Transport.Patient;

public interface IPatient
{
    Id Id { get; }
    Name Name { get; }
    ZodiacSign? ZodiacSign { get; }
    IPhoneNumber PhoneNumber { get; }
    IMailAddress MailAddress { get; }
}

public record Patient(
    Id Id,
    Name Name,
    ZodiacSign? ZodiacSign,
    IPhoneNumber PhoneNumber,
    IMailAddress MailAddress
) : IPatient;

public interface IPhoneNumber { }

public record PhoneNumber(string Value)
    : IPhoneNumber
{
    public override string ToString() => Value;
}

public interface IMailAddress { }

public record MailAddress(string Value)
    : IMailAddress
{
    public override string ToString() => Value;
}
