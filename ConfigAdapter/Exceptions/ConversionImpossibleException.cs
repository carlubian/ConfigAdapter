using System;

namespace ConfigAdapter.Exceptions
{
    /// <summary>
    /// Value cannot be converted to
    /// the specified type.
    /// </summary>
    [Serializable]
    public class ConversionImpossibleException : ApplicationException
    {
        public ConversionImpossibleException(string message) : base(message)
        {
        }

        public ConversionImpossibleException() : base()
        {
        }
    }
}
