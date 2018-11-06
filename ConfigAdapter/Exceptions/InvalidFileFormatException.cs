using System;

namespace ConfigAdapter.Exceptions
{
    /// <summary>
    /// The configuration file has an
    /// incorrect file format.
    /// </summary>
    [Serializable]
    public class InvalidFileFormatException : ApplicationException
    {
        public InvalidFileFormatException(string message) : base(message)
        {
        }

        public InvalidFileFormatException() : base()
        {
        }
    }
}
