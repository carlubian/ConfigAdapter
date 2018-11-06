using System;

namespace ConfigAdapter.Exceptions
{
    /// <summary>
    /// The configuration file has an unknown or
    /// otherwise unidentifiable format.
    /// </summary>
    [Serializable]
    public class UnknownFileFormatException : ApplicationException
    {
        public UnknownFileFormatException(string message) : base(message)
        {
        }

        public UnknownFileFormatException() : base()
        {
        }
    }
}
