using ConfigAdapter.Exceptions;
using ConfigAdapter.Extensions;
using ConfigAdapter.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DotNet.Misc.Extensions.Linq;

namespace ConfigAdapter.Adapters
{
    /// <summary>
    /// Connects to XML
    /// configuration files.
    /// </summary>
    public class XmlFileAdapter : IFileAdapter, ITransferable
    {
        private readonly string _file;
        private readonly static string[] _formats = { "A" };
        private XElement _root;

        public XmlFileAdapter(string file)
        {
            _file = file;

            try
            { Directory.CreateDirectory(Path.GetDirectoryName(_file)); }
            catch (ArgumentException)
            { /* File is within local directory. */}

            if (!File.Exists(file))
                new XElement("Configuration", 
                    new XAttribute("Format", "A"))
                    .Save(file);

            _root = XElement.Load(_file);

            var format = _root.Attributes()
                .FirstOrDefault(a => a.Name == "Format").Value ?? "No format";
            if (!_formats.Contains(format))
                throw new IncompatibleXmlFormatException($"Format {format} cannot be used from this version.");
        }

        public string Read(string key)
        {
            var parts = key.Split(':');
            // Global key
            if (parts.Length is 1)
                return _root.Elements()
                    .Where(e => e.Attribute("Key")?.Value == parts[0])
                    .FirstOrDefault()?.Value;
            // Local key
            else if (parts.Length is 2)
                return _root.Elements(parts[0])
                    .SelectMany(cat => cat.Elements("Setting"))
                    .Where(e => e.Attribute("Key")?.Value == parts[1])
                    .FirstOrDefault()?.Value;

            throw new InvalidKeyFormatException($"Key {key} has an incorrect format.");
        }

        public void Write(string key, string value, string comment = null)
        {
            var parts = key.Split(':');

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

        IList<Setting> ITransferable.ReadAll()
        {
            var result = new List<Setting>();

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

            return result;
        }

        void ITransferable.WriteAll(IList<Setting> source)
        {
            foreach (var setting in source)
                Write(setting.Key, setting.Value, setting.Comment);
        }
    }
}
