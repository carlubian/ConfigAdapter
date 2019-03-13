using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConfigAdapter.Adapters;
using ConfigAdapter.Exceptions;
using ConfigAdapter.Extensions;
using ConfigAdapter.Model;
using Hjson;

namespace ConfigAdapter.HJson
{
    /// <summary>
    /// Connects to a JSON
    /// configuration file.
    /// </summary>
    public class HJsonFileAdapter : IFileAdapter, ITransferable
    {
        private readonly string _file;
        private WscJsonObject _content;

        public HJsonFileAdapter(string file)
        {
            if (!file.EndsWith(".hjson"))
                throw new InvalidFileFormatException(".hjson file extension required.");

            _file = file;

            try
            { Directory.CreateDirectory(Path.GetDirectoryName(_file)); }
            catch (ArgumentException)
            { /* File is within local directory. */}

            if (!File.Exists(file))
                HjsonValue.Save(new JsonObject(), _file);

            _content = (WscJsonObject)HjsonValue.Load(_file, new HjsonOptions
            {
                KeepWsc = true
            }).Qo();
        }

        public string Read(string key)
        {
            var parts = key.Split(':');

            try
            {
                // Global key
                if (parts.Length is 1)
                {
                    return _content[parts[0]];
                }
                // Local key
                else if (parts.Length is 2)
                {
                    return _content[parts[0]][parts[1]];
                }
            }
            catch
            {
                return null;
            }
            
            throw new InvalidKeyFormatException($"La clave {key} tiene un formato incorrecto.");
        }

        public void Write(string key, string value, string comment = null)
        {
            var parts = key.Split(':');

            // Global key
            if (parts.Length is 1)
            {
                _content.Add(parts[0], value);

                // Add comment if present
                if (comment != null)
                {
                    RefreshFile();

                    _content.Comments[parts[0]] = comment;
                }
            }
            // Local key
            else if (parts.Length is 2)
            {
                // Nonexistant category
                if (!_content.Keys.Contains(parts[0]))
                {
                    _content.Add(parts[0], new JsonObject());
                }

                _content[parts[0]].Qo().Add(parts[1], value);

                // Add comment if present
                if (comment != null)
                {
                    RefreshFile();

                    ((WscJsonObject)_content[parts[0]].Qo())
                        .Comments[parts[1]] = comment;
                }
            }
            else
                throw new InvalidKeyFormatException($"La clave {key} tiene un formato incorrecto.");

            HjsonValue.Save(_content, _file, new HjsonOptions
            {
                KeepWsc = true
            });
        }

        public void DeleteKey(string key)
        {
            var parts = key.Split(':');

            // Global key
            if (parts.Length is 1)
            {
                _content.Remove(parts[0]);
            }
            // Local key
            else if (parts.Length is 2)
            {
                _content[parts[0]].Qo().Remove(parts[1]);
            }
            else
                throw new InvalidKeyFormatException($"La clave {key} tiene un formato incorrecto.");

            HjsonValue.Save(_content, _file, new HjsonOptions
            {
                KeepWsc = true
            });
        }

        public void DeleteSection(string section)
        {
            _content.Remove(section);

            HjsonValue.Save(_content, _file, new HjsonOptions
            {
                KeepWsc = true
            });
        }

        /// <summary>
        /// Update file content to correctly
        /// handle comments.
        /// </summary>
        private void RefreshFile()
        {
            HjsonValue.Save(_content, _file, new HjsonOptions
            {
                KeepWsc = true
            });
            _content = (WscJsonObject)HjsonValue.Load(_file, new HjsonOptions
            {
                KeepWsc = true
            }).Qo();
        }

        IList<Setting> ITransferable.ReadAll()
        {
            var result = new List<Setting>();

            _content.ToList().ForEach(kvp =>
            {
                if (kvp.Value.JsonType is JsonType.String
                || kvp.Value.JsonType is JsonType.Number)
                    result.Add(new Setting(kvp.Key, kvp.Value.Qstr(), _content.Comments[kvp.Key]));

                if (kvp.Value.JsonType is JsonType.Object)
                    kvp.Value.Qo().ToList().ForEach(ckvp =>
                    {
                        result.Add(new Setting($"{kvp.Key}:{ckvp.Key}", ckvp.Value.Qstr(), (kvp.Value as WscJsonObject).Comments[ckvp.Key]));
                    });
            });

            return result;
        }

        void ITransferable.WriteAll(IList<Setting> source)
        {
            foreach (var setting in source)
                Write(setting.Key, setting.Value, setting.Comment);
        }

        public IDictionary<string, string> SettingsIn(string section)
        {
            var result = new Dictionary<string, string>();

            if (section is "")
            {
                // Global settings
                var elems = _content.Where(kvp => kvp.Value.JsonType != JsonType.Object);
                foreach (var kvp in elems.ToArray())
                    result.Add(kvp.Key, kvp.Value.Qs());
            }
            else
            {
                // Specific settings
                var elems = _content[section];
                foreach (var kvp in (elems as WscJsonObject).ToArray())
                    result.Add(kvp.Key, kvp.Value.Qs());
            }

            return result;
        }
    }
}
