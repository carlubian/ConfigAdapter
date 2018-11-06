using System;

namespace ConfigAdapter.Exceptions
{
    /// <summary>
    /// This XML file has an incompatible format.
    /// </summary>
    [Serializable]
    public class IncompatibleXmlFormatException : ApplicationException
    {
        public IncompatibleXmlFormatException(string message) : base(message)
        {
        }

        public IncompatibleXmlFormatException() : base()
        {
        }
    }
}
