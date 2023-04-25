namespace Transport.Patient;

public record Patient(
    Id Id,
    Name Name,
    ZodiacSign? ZodiacSign,
    PhoneNumber PhoneNumber,
    MailAddress MailAddress
);

public record PhoneNumber(string Value)
{
    public override string ToString() => Value;
}

public record MailAddress(string Value)
{
    public override string ToString() => Value;
}