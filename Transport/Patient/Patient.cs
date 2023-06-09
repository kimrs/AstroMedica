﻿namespace Transport.Patient;

public record Patient(
    Id Id,
    Name Name,
    ZodiacSign? ZodiacSign,
    PhoneNumber PhoneNumber,
    MailAddress MailAddress
);

public class PhoneNumber
{
    public string Value { get; }

    public PhoneNumber(string value)
    {
        Value = value;
    }
    public override string ToString() => Value;
}

public record MailAddress(string Value)
{
    public override string ToString() => Value;
}