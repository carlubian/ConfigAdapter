using ConfigAdapter.Exceptions;
using ConfigAdapter.Extensions;
using ConfigAdapter.Adapters;
using ConfigAdapter.Model;
using System;
using System.Globalization;
using System.Collections.Generic;

namespace ConfigAdapter
{
    /// <summary>
    /// Entry Point for configuration handling.
    /// </summary>
    public class Config
    {
        private readonly IFileAdapter _file;

        public Config(IFileAdapter file)
        {
            _file = file;
        }

        /// <summary>
        /// Returns the value assigned to a
        /// key (or null if missing).
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        public string Read(string key)
        {
            return _file.Read(key);
        }

        /// <summary>
        /// Returns the value of a key converted
        /// into a primitive type, if possible.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        public T Read<T>(string key) where T : struct
        {
            try
            {
                if (typeof(T) == 0D.GetType())
                    return (T)Convert.ChangeType(_file.Read(key).ToDouble(), typeof(T));
                if (typeof(T) == 0F.GetType())
                    return (T)Convert.ChangeType(_file.Read(key).ToFloat(), typeof(T));
                return (T)Convert.ChangeType(_file.Read(key), typeof(T));
            }
            catch
            {
                throw new ConversionImpossibleException($"Value of {key} cannot be converted to {typeof(T).Name}");
            }
        }

        /// <summary>
        /// Writes a key, a value, and optionally a comment.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <param name="comment">Comment</param>
        public Config Write(string key, string value, string comment = null)
        {
            _file.Write(key, value, comment);

            return this;
        }

        /// <summary>
        /// Escribe una clave y su valor como
        /// tipo primitivo, con un comentario opcional.
        /// Writes a key, a value as a primitive type,
        /// and optionally a comment.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <param name="comment">Comment</param>
        public Config Write<T>(string key, T value, string comment = null) where T : struct
        {
            if (typeof(T) == 0D.GetType())
                _file.Write(key, ((double)Convert.ChangeType(value, typeof(double))).ToString(CultureInfo.InvariantCulture), comment);
            if (typeof(T) == 0F.GetType())
                _file.Write(key, ((float)Convert.ChangeType(value, typeof(float))).ToString(CultureInfo.InvariantCulture), comment);
            _file.Write(key, value.ToString(), comment);

            return this;
        }

        public Config DeleteKey(string key)
        {
            _file.DeleteKey(key);
            return this;
        }

        public Config DeleteSection(string section)
        {
            _file.DeleteSection(section);
            return this;
        }

        /// <summary>
        /// Returns all settings inside the specified
        /// section (or global, if section is an empty string).
        /// </summary>
        /// <param name="section">Section name</param>
        /// <returns>Settings in section</returns>
        public IDictionary<string, string> SettingsIn(string section)
        {
            return _file.SettingsIn(section);
        }

        /// <summary>
        /// Transfers all settings in this file into a new
        /// file, not necessarily in the same format.
        /// </summary>
        /// <param name="destination">Destination adapter</param>
        /// <returns></returns>
        public Config TransferTo(ITransferable destination)
        {
            var data = (_file as ITransferable).ReadAll();
            destination.WriteAll(data);

            return this;
        }

        /// <summary>
        /// Transfers all settings in this file into a new
        /// file, not necessarily in the same format.
        /// </summary>
        /// <param name="config">Destination config</param>
        /// <returns></returns>
        public Config TransferTo(Config config)
        {
            var destination = config._file as ITransferable;
            return TransferTo(destination);
        }

        /// <summary>
        /// Returns this instance of Config as an
        /// ITransferable to be used in file transfers.
        /// </summary>
        /// <returns></returns>
        public ITransferable AsTransferable()
        {
            return _file as ITransferable;
        }
    }
}
