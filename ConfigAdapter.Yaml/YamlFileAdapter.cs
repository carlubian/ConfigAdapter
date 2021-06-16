using ConfigAdapter.Adapters;
using ConfigAdapter.Exceptions;
using ConfigAdapter.Model;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace ConfigAdapter.Yaml
{
    public class YamlFileAdapter : IFileAdapter, ITransferable
    {
        private IDictionary<string, object> _File;
        private string path;

        public YamlFileAdapter(string file)
        {
            var deserializer = new Deserializer();
            var text = File.Exists(file) ? File.ReadAllText(file) : "";

            path = file;
            _File = deserializer.Deserialize<IDictionary<string, object>>(text);
        }

        public string Read(string key)
        {
            // Case A: Global setting
            if (!key.Contains(":"))
            {
                if (_File.ContainsKey(key))
                    return _File[key] as string;
                else
                    return "";
            }
            // Case B: Local setting
            else
            {
                var fragments = key.Split(":");
                if (fragments.Length != 2)
                    throw new InvalidKeyFormatException();

                var sectionKey = fragments[0];
                var settingKey = fragments[1];

                if (!_File.ContainsKey(sectionKey))
                    return "";

                var section = _File[sectionKey] as IDictionary<object, object>;

                if (!section.ContainsKey(settingKey))
                    return "";
                else
                    return section[settingKey].ToString();
            }
        }

        public void Write(string key, string value, string comment = null)
        {
            // Case A: Global setting
            if (!key.Contains(":"))
            {
                if (_File.ContainsKey(key))
                    _File.Remove(key);

                _File.Add(key, value);
            }
            // Case B: Local setting
            else
            {
                var fragments = key.Split(":");
                if (fragments.Length != 2)
                    throw new InvalidKeyFormatException();

                var sectionKey = fragments[0];
                var settingKey = fragments[1];

                if (!_File.ContainsKey(sectionKey))
                    _File.Add(sectionKey, new Dictionary<object, object>());

                var section = _File[sectionKey] as IDictionary<object, object>;
                if (section.ContainsKey(settingKey))
                    section.Remove(settingKey);

                section.Add(settingKey, value);
            }

            SaveAndRefresh();
        }

        public void DeleteKey(string key)
        {
            // Case A: Global section
            if (!key.Contains(":"))
            {
                if (_File.ContainsKey(key))
                    _File.Remove(key);
            }
            // Case B: Local section
            else
            {
                var fragments = key.Split(":");
                if (fragments.Length != 2)
                    throw new InvalidKeyFormatException();

                var sectionKey = fragments[0];
                var settingKey = fragments[1];

                if (!_File.ContainsKey(sectionKey))
                    return;

                var section = _File[sectionKey] as IDictionary<object, object>;
                if (section.ContainsKey(settingKey))
                    section.Remove(settingKey);
            }

            SaveAndRefresh();
        }

        public void DeleteSection(string section)
        {
            if (_File.ContainsKey(section))
                _File.Remove(section);

            SaveAndRefresh();
        }

        public IList<Setting> ReadAll()
        {
            var result = new List<Setting>();

            foreach (var kvp in _File)
            {
                if (kvp.Value is IDictionary<object, object> section)
                    foreach (var kvp2 in section)
                        result.Add(new Setting
                        {
                            Key = $"{kvp.Key}:{kvp2.Key}",
                            Value = kvp2.Value.ToString()
                        });
                else
                    result.Add(new Setting
                    {
                        Key = kvp.Key,
                        Value = kvp.Value.ToString()
                    });
            }

            return result;
        }

        public void WriteAll(IList<Setting> source)
        {
            foreach (var setting in source)
                Write(setting.Key, setting.Value, setting.Comment);
        }

        public IDictionary<string, string> SettingsIn(string section)
        {
            // Case A: Global section
            if (section is "")
            {
                return _File.Where(kvp => kvp.Value is not IDictionary<object, object>)
                    .Aggregate(new Dictionary<string, string>(), (dict, kvp) =>
                    {
                        dict.Add(kvp.Key.ToString(), kvp.Value.ToString());
                        return dict;
                    });
            }
            // Case B: Local section
            else
            {
                if (!_File.ContainsKey(section))
                    return ImmutableDictionary<string, string>.Empty;

                var internalSection = _File[section] as IDictionary<object, object>;
                return internalSection.Aggregate(new Dictionary<string, string>(), (dict, kvp) =>
                {
                    dict.Add(kvp.Key.ToString(), kvp.Value.ToString());
                    return dict;
                });
            }
        }

        internal void SaveAndRefresh()
        {
            var serializer = new Serializer();
            var newText = serializer.Serialize(_File);

            File.Delete(path);
            File.WriteAllText(path, newText);

            var deserializer = new Deserializer();
            var text = File.Exists(path) ? File.ReadAllText(path) : "";
            _File = deserializer.Deserialize<IDictionary<string, object>>(text);
        }
    }
}
