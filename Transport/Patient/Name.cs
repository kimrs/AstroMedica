namespace Transport.Patient;

public record Name
{
    private string _value;
    public Name(string value)
    {
        _value = value.Length is > 0 and < 100
            ? value
            : throw new ArgumentException($"{typeof(Name)} must have between 0 and 100 characters");
    }

    public string Value => _value;

    public override string ToString()
    {
        return _value;
    }
}