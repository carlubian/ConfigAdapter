namespace ConfigAdapter.Core.Exceptions;

/// <summary>
/// Indicates an attempt to operate with a configuration provider
/// that is not ready or properly set up.
/// </summary>
public class InconsistentStateException : ApplicationException
{
    public override string Message => "This provider is not correctly initialized. Make sure to access it from the Configuration class, and not creating it directly.";
}
