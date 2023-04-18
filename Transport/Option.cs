namespace Transport;

public interface IOption
{
}

public record Some<T>(T Value) : IOption;

public record None(ReasonForNone Because) : IOption;

public enum ReasonForNone { ServiceUnavailable, ServiceNotYetInitialized, PatientDoesNotExist}