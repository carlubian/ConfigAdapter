using System;

namespace ConfigAdapter.Exceptions
{
    /// <summary>
    /// The specified key has an
    /// incorrect format.
    /// </summary>
    [Serializable]
    public class InvalidKeyFormatException : ApplicationException
    {
        public InvalidKeyFormatException(string message) : base(message)
        {
        }

        public InvalidKeyFormatException() : base()
        {
        }
    }
}
