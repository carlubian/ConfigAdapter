﻿using ConfigAdapter.Exceptions;
using ConfigAdapter.Extensions;
using ConfigAdapter.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DotNet.Misc.Extensions.Linq;
using ConfigAdapter.Adapters;

namespace ConfigAdapter.Xml
{
    /// <summary>
    /// Connects to XML
    /// configuration files.
    /// </summary>
    public class XmlFileAdapter : IFileAdapter, ITransferable
    {
        private readonly string _file;
        private readonly static string[] _formats = { "A", "CAv2" };
        private readonly XElement _root;
        private string _thisFormat;

        public XmlFileAdapter(string file)
        {
            _file = file;

            try
            { Directory.CreateDirectory(Path.GetDirectoryName(_file)); }
            catch (ArgumentException)
            { /* File is within local directory. */}

            if (!File.Exists(file))
                new XElement("Configuration", 
                    new XAttribute("Format", "CAv2"))
                    .Save(file);

            _root = XElement.Load(_file);

            _thisFormat = _root.Attributes()
                .FirstOrDefault(a => a.Name == "Format").Value ?? "No format";
            if (!_formats.Contains(_thisFormat))
                throw new IncompatibleXmlFormatException($"Format {_thisFormat} cannot be used from this version.");
        }

        public string Read(string key)
        {
            var parts = key.Split(':');
            if (_thisFormat is "A")
            {
                // Global key
                if (parts.Length is 1)
                    return _root.Elements()
                        .FirstOrDefault(e => e.Attribute("Key")?.Value == parts[0])?.Value;
                // Local key
                else if (parts.Length is 2)
                    return _root.Elements(parts[0])
                        .SelectMany(cat => cat.Elements("Setting"))
                        .FirstOrDefault(e => e.Attribute("Key")?.Value == parts[1])?.Value;

                throw new InvalidKeyFormatException($"Key {key} has an incorrect format.");
            }
            else if (_thisFormat is "CAv2")
            {
                // Global key
                if (parts.Length is 1)
                    return _root.Elements()
                        .FirstOrDefault(e => e.Name == parts[0])?.Value;
                // Local key
                else if (parts.Length is 2)
                    return _root.Elements(parts[0])
                        .SelectMany(cat => cat.Elements())
                        .FirstOrDefault(e => e.Name == parts[1])?.Value;

                throw new InvalidKeyFormatException($"Key {key} has an incorrect format.");
            }
            else
                throw new IncompatibleXmlFormatException($"Format {_thisFormat} cannot be used from this version.");
        }

        public void Write(string key, string value, string comment = null)
        {
            var parts = key.Split(':');

            if (_thisFormat is "A")
            {
                // Global key
                if (parts.Length is 1)
                {
                    // Overwrite existing setting if present
                    var elem = _root.Descendants()
                        .FirstOrDefault(e => e.Attribute("Key")?.Value == parts[0]);

                    // Nonexistant setting
                    if (elem is null)
                    {
                        var clave = new XElement("Setting",
                            new XAttribute("Key", parts[0]),
                            value);
                        _root.Add(clave);

                        if (comment != null)
                            clave.AddBeforeSelf(new XComment(comment));
                    }
                    // Already present setting
                    else
                        elem.Value = value;

                    _root.Save(_file);
                }
                // Local key
                else if (parts.Length is 2)
                {
                    // Insert in existing category if present
                    var cat = _root.Descendants()
                        .FirstOrDefault(n => n.Name == parts[0]);

                    // Nonexistant category
                    if (cat is null)
                    {
                        if (comment != null)
                        {
                            var clave = new XElement(parts[0],
                                new XComment(comment),
                                new XElement("Setting",
                                    new XAttribute("Key", parts[1]),
                                    value));
                            _root.Add(clave);
                        }
                        else
                        {
                            var clave = new XElement(parts[0],
                                new XElement("Setting",
                                    new XAttribute("Key", parts[1]),
                                    value));
                            _root.Add(clave);
                        }
                    }
                    // Already present category
                    else
                    {
                        // Overwrite existing setting if present
                        var elem = cat.Descendants()
                            .FirstOrDefault(e => e.Attribute("Key")?.Value == parts[1]);

                        // Nonexistant setting
                        if (elem is null)
                        {
                            if (comment != null)
                                cat.Add(new XComment(comment));

                            var clave = new XElement("Setting",
                                    new XAttribute("Key", parts[1]),
                                    value);
                            cat.Add(clave);
                        }
                        // Already existing setting
                        else
                        {
                            // Add comment if present
                            if (comment != null)
                            {
                                var cmt = elem.PreviousNode?.NodeType ==
                                    System.Xml.XmlNodeType.Comment ?
                                    elem.PreviousNode : null;

                                if (cmt != null)
                                    cmt.ReplaceWith(new XComment(comment));
                                else
                                    elem.AddBeforeSelf(new XComment(comment));
                            }
                            elem.Value = value;
                        }

                    }
                    _root.Save(_file);
                }
                else
                    throw new InvalidKeyFormatException($"Key {key} has an incorrect format.");
            }
            else if (_thisFormat is "CAv2")
            {
                // Global key
                if (parts.Length is 1)
                {
                    // Overwrite existing setting if present
                    var elem = _root.Descendants()
                        .FirstOrDefault(e => e.Name == parts[0]);

                    // Nonexistant setting
                    if (elem is null)
                    {
                        var clave = new XElement(parts[0],
                            value);
                        _root.Add(clave);

                        if (comment != null)
                            clave.AddBeforeSelf(new XComment(comment));
                    }
                    // Already present setting
                    else
                        elem.Value = value;

                    _root.Save(_file);
                }
                // Local key
                else if (parts.Length is 2)
                {
                    // Insert in existing category if present
                    var cat = _root.Descendants()
                        .FirstOrDefault(n => n.Name == parts[0]);

                    // Nonexistant category
                    if (cat is null)
                    {
                        if (comment != null)
                        {
                            var clave = new XElement(parts[0],
                                new XComment(comment),
                                new XElement(parts[1],
                                    value));
                            _root.Add(clave);
                        }
                        else
                        {
                            var clave = new XElement(parts[0],
                                new XElement(parts[1],
                                    value));
                            _root.Add(clave);
                        }
                    }
                    // Already present category
                    else
                    {
                        // Overwrite existing setting if present
                        var elem = cat.Descendants()
                            .FirstOrDefault(e => e.Name == parts[1]);

                        // Nonexistant setting
                        if (elem is null)
                        {
                            if (comment != null)
                                cat.Add(new XComment(comment));

                            var clave = new XElement(parts[1],
                                    value);
                            cat.Add(clave);
                        }
                        // Already existing setting
                        else
                        {
                            // Add comment if present
                            if (comment != null)
                            {
                                var cmt = elem.PreviousNode?.NodeType ==
                                    System.Xml.XmlNodeType.Comment ?
                                    elem.PreviousNode : null;

                                if (cmt != null)
                                    cmt.ReplaceWith(new XComment(comment));
                                else
                                    elem.AddBeforeSelf(new XComment(comment));
                            }
                            elem.Value = value;
                        }

                    }
                    _root.Save(_file);
                }
                else
                    throw new InvalidKeyFormatException($"Key {key} has an incorrect format.");
            }
            else
                throw new IncompatibleXmlFormatException($"Format {_thisFormat} cannot be used from this version.");
        }

        public void DeleteKey(string key)
        {
            var parts = key.Split(':');

            if (_thisFormat is "A")
            {
                // Global key
                if (parts.Length is 1)
                {
                    // Find key
                    var elem = _root.Descendants()
                        .FirstOrDefault(e => e.Attribute("Key")?.Value == parts[0]);

                    if (elem != null)
                        elem.Remove();

                    _root.Save(_file);
                }
                // Local key
                else if (parts.Length is 2)
                {
                    // Find category
                    var cat = _root.Descendants()
                         .FirstOrDefault(n => n.Name == parts[0]);

                    if (cat != null)
                    {
                        // Find key
                        var elem = cat.Descendants()
                            .FirstOrDefault(e => e.Attribute("Key")?.Value == parts[1]);

                        if (elem != null)
                            elem.Remove();
                    }

                    _root.Save(_file);
                }
                else
                    throw new InvalidKeyFormatException($"Key {key} has an incorrect format.");
            }
            else if (_thisFormat is "CAv2")
            {
                // Global key
                if (parts.Length is 1)
                {
                    // Find key
                    var elem = _root.Descendants()
                        .FirstOrDefault(e => e.Name == parts[0]);

                    if (elem != null)
                        elem.Remove();

                    _root.Save(_file);
                }
                // Local key
                else if (parts.Length is 2)
                {
                    // Find category
                    var cat = _root.Descendants()
                         .FirstOrDefault(n => n.Name == parts[0]);

                    if (cat != null)
                    {
                        // Find key
                        var elem = cat.Descendants()
                            .FirstOrDefault(e => e.Name == parts[1]);

                        if (elem != null)
                            elem.Remove();
                    }

                    _root.Save(_file);
                }
                else
                    throw new InvalidKeyFormatException($"Key {key} has an incorrect format.");
            }
            else
                throw new IncompatibleXmlFormatException($"Format {_thisFormat} cannot be used from this version.");

        }

        public void DeleteSection(string section)
        {
            // Find category
            var cat = _root.Descendants()
                    .FirstOrDefault(n => n.Name == section);

            if (cat != null)
                cat.Remove();

            _root.Save(_file);
        }

        IList<Setting> ITransferable.ReadAll()
        {
            var result = new List<Setting>();

            if (_thisFormat is "A")
            {
                result.AddRange(_root.Elements()
                            .Where(e => e.Attribute("Key") != null)
                            .Select(e =>
                            {
                                var key = e.Attribute("Key").Value;
                                var value = e.FirstNode.ToString();
                                var comment = e.PreviousNode?.NodeType is System.Xml.XmlNodeType.Comment
                                            ? (e.PreviousNode as XComment).Value
                                            : null;

                                return new Setting(key, value, comment);
                            }));

                _root.Descendants().Where(e => e.Attribute("Key") is null)
                    .ForEach(cat =>
                    {
                        result.AddRange(cat.Elements()
                                .Where(e => e.Attribute("Key") != null)
                                .Select(e =>
                                {
                                    var key = $"{cat.Name}:{e.Attribute("Key").Value}";
                                    var value = e.FirstNode.ToString();
                                    var comment = e.PreviousNode?.NodeType is System.Xml.XmlNodeType.Comment
                                                ? (e.PreviousNode as XComment).Value
                                                : null;

                                    return new Setting(key, value, comment);
                                }));
                    });
            }
            else if (_thisFormat is "CAv2")
            {
                result.AddRange(_root.Elements()
                            .Where(e => e.HasElements is false)
                            .Select(e =>
                            {
                                var key = e.Name.ToString();
                                var value = e.Value;
                                var comment = e.PreviousNode?.NodeType is System.Xml.XmlNodeType.Comment
                                            ? (e.PreviousNode as XComment).Value
                                            : null;

                                return new Setting(key, value, comment);
                            }));

                _root.Descendants()
                    .ForEach(cat =>
                    {
                        result.AddRange(cat.Elements()
                                .Where(e => e.HasElements is false)
                                .Select(e =>
                                {
                                    var key = $"{cat.Name}:{e.Name.ToString()}";
                                    var value = e.Value;
                                    var comment = e.PreviousNode?.NodeType is System.Xml.XmlNodeType.Comment
                                                ? (e.PreviousNode as XComment).Value
                                                : null;

                                    return new Setting(key, value, comment);
                                }));
                    });
            }
            else
                throw new IncompatibleXmlFormatException($"Format {_thisFormat} cannot be used from this version.");

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

            if (_thisFormat is "A")
            {
                if (section is "")
                {
                    // Global section
                    var elems = _root.Elements()
                        .Where(e => e.Attribute("Key") != null);
                    foreach (var elem in elems)
                        result.Add(elem.Attribute("Key")?.Value, elem.Value);
                }
                else
                {
                    // Specific section
                    var elems = _root.Elements(section)
                        .SelectMany(cat => cat.Elements("Setting"));
                    foreach (var elem in elems)
                        result.Add(elem.Attribute("Key")?.Value, elem.Value);
                }
            }
            else if (_thisFormat is "CAv2")
            {
                if (section is "")
                {
                    // Global section
                    var elems = _root.Elements()
                        .Where(e => e.HasElements is false);
                    foreach (var elem in elems)
                        result.Add(elem.Name.ToString(), elem.Value);
                }
                else
                {
                    // Specific section
                    var elems = _root.Elements(section)
                        .SelectMany(cat => cat.Elements());
                    foreach (var elem in elems)
                        result.Add(elem.Name.ToString(), elem.Value);
                }
            }
            else
                throw new IncompatibleXmlFormatException($"Format {_thisFormat} cannot be used from this version.");

            return result;
        }
    }
}
