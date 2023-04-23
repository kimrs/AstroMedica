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

public record None<T>(ReasonForNone Because) : IOption<T>
{
    public T EnsureHasValue()
    {
        var message = Because switch
        {
            ReasonForNone.ServiceUnavailable => "Service unavailable, message placed on the error queue",
            ReasonForNone.ServiceNotYetInitialized => "Service is still initializing. Retrying in a couple of minutes",
            ReasonForNone.ItemDoesNotExist => "Item does not exist",
            _ => "An error occured"
        };
        throw new Exception(message);
    }
}

public enum ReasonForNone { ServiceUnavailable, ServiceNotYetInitialized, ItemDoesNotExist, FailedToDeserialize}