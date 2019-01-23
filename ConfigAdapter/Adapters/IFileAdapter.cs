
using System.Collections.Generic;

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

        /// <summary>
        /// Returns all settings present in the
        /// given section (or the global section
        /// if the parameter string is empty).
        /// </summary>
        /// <param name="section">Section name</param>
        /// <returns>Settings in the section</returns>
        IDictionary<string, string> SettingsIn(string section);
    }
}
