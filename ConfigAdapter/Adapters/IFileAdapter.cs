
namespace ConfigAdapter.Adapters
{
    /// <summary>
    /// Represent an adapter for configuration
    /// files, independently of their
    /// specific file format.
    /// </summary>
    public interface IFileAdapter
    {
        /// <summary>
        /// Reads and returns the value of a key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        string Read(string key);

        /// <summary>
        /// Writes the value of a key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        void Write(string key, string value, string comment = null);
    }
}
