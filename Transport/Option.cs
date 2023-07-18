namespace Transport;

public interface IOption<out T>
{
    T EnsureHasValue();
}

public record Some<T>(T Value) : IOption<T>
{
    public T EnsureHasValue()
    {
        if (Value is null)
        {
            throw new NullReferenceException(nameof(Value));
        }

        return Value;
    }
}

public record None<T>(IReason Because) : IOption<T>
{
    public T EnsureHasValue()
    {
        throw Because.Exception;
    }
}

public interface IReason
{
    Exception Exception { get; }
}

public record ServiceUnavailable
    : IReason
{
    public Exception Exception => new("Service unavailable, message placed on the error queue");
}

public record ServiceNotYetInitialized
    : IReason
{
    public Exception Exception => new("Service is still initializing. Retrying in a couple of minutes");
}

public record ItemDoesNotExist
    : IReason
{
    public Exception Exception => new("Item does not exist");
}

public record FailedToDeserialize
    : IReason
{
    public Exception Exception => new("Failed to deserialize object");
}