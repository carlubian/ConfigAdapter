using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConfigAdapter.Exceptions;
using ConfigAdapter.Extensions;
using ConfigAdapter.Model;
using IniParser;
using IniParser.Model;
using IniParser.Parser;
using DotNet.Misc.Extensions.Linq;
using ConfigAdapter.Adapters;

namespace ConfigAdapter.Ini
{
    /// <summary>
    /// Connects to INI format
    /// configuration files.
    /// </summary>
    public class IniFileAdapter : IFileAdapter, ITransferable
    {
        private readonly string _file;
        private readonly IniData _ini;

        public IniFileAdapter(string file)
        {
            if (!file.EndsWith(".ini"))
                throw new InvalidFileFormatException(".ini file extension required.");

            _file = file;

            try
            { Directory.CreateDirectory(Path.GetDirectoryName(_file)); }
            catch (ArgumentException)
            { /* File is within local directory. */}

            if (!File.Exists(file))
                new FileIniDataParser().WriteFile(file, new IniData());

            _ini = new IniDataParser().Parse(File.ReadAllText(_file));
        }

        public string Read(string key)
        {
            var parts = key.Split(':');
            // Global key
            if (parts.Length is 1)
                return _ini.GetKey(parts[0]);
            // Local key
            else if (parts.Length is 2)
                return _ini[parts[0]][parts[1]];

            throw new InvalidKeyFormatException($"Key {key} has an incorrect format.");
        }

        public IDictionary<string, string> SettingsIn(string section)
        {
            var result = new Dictionary<string, string>();

            if (section is "")
            {
                // Global settings
                var elems = _ini.Global;
                foreach (var kvp in elems)
                    result.Add(kvp.KeyName, kvp.Value);
            }
            else
            {
                // Specific settings
                var elems = _ini[section];
                foreach (var kvp in elems)
                    result.Add(kvp.KeyName, kvp.Value);
            }

            return result;
        }

        public void Write(string key, string value, string comment = null)
        {
            var parts = key.Split(':');

            // Global Key
            if (parts.Length is 1)
            {
                _ini.Global[parts[0]] = value;
                _ini.Global.GetKeyData(parts[0]).Comments = comment.Enumerate().ToList();
                new FileIniDataParser().WriteFile(_file, _ini);
            }
            // Local key
            else if (parts.Length is 2)
            {
                _ini[parts[0]][parts[1]] = value;
                _ini[parts[0]].GetKeyData(parts[1]).Comments = comment.Enumerate().ToList();
                new FileIniDataParser().WriteFile(_file, _ini);
            }
            else
                throw new InvalidKeyFormatException($"Key {key} has an incorrect format.");
        }

        IList<Setting> ITransferable.ReadAll()
        {
            var result = new List<Setting>();

            foreach (var kvp in _ini.Global)
                result.Add(new Setting(kvp.KeyName, kvp.Value, 
                    _ini.Global.GetKeyData(kvp.KeyName).Comments.FirstOrDefault()));

            foreach (var section in _ini.Sections)
                foreach (var kvp in section.Keys)
                    result.Add(new Setting($"{section.SectionName}:{kvp.KeyName}", kvp.Value, 
                        _ini[section.SectionName].GetKeyData(kvp.KeyName).Comments.FirstOrDefault()));

            return result;
        }

        void ITransferable.WriteAll(IList<Setting> source)
        {
            foreach (var setting in source)
                Write(setting.Key, setting.Value, setting.Comment);
        }
    }
}
