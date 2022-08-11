namespace ConfigAdapter.Core.Exceptions;

/// <summary>
/// Indicates a format error trying to open or parse a file.
/// </summary>
public class IncorrectFormatException : ApplicationException
{
    public override string Message => "The file has incorrect format or metadata entries.";
}
