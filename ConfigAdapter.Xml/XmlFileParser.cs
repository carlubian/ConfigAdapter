using ConfigAdapter.Core.Exceptions;
using ConfigAdapter.Core.Model;
using System.Xml.Linq;

namespace ConfigAdapter.Xml
{
    internal static class XmlFileParser
    {
        internal static ConfigAdapterFile ParseFile(string path)
        {
            var document = XElement.Load(path);

            var result = new ConfigAdapterFile()
            {
                FileExtension = "XML",
                FileName = new FileInfo(path).Name,
                Sections = new List<ConfigAdapterSection>()
            };

            // Check metadata for valid format
            if (!HasValidFormat(document))
                throw new IncorrectFormatException();

            // Fill global settings
            result.Sections.Add(PopulateGlobalSettings(document));

            // Fill sections
            var contentNode = FindContentNode(document);
            foreach (var child in contentNode.Elements())
            {
                if (child.Name == "Section")
                    result.Sections.Add(PopulateSection(child));
            }

            return result;
        }

        internal static void PersistFile(ConfigAdapterFile root, string path)
        {
            var document = new XElement("Configuration");
            CreateFramework(document);

            PersistGlobalSettings(document, root);

            foreach (var section in root.Sections)
                PersistLocalSection(document.Elements().Single(e => e.Name == "Content"), section);

            document.Save(path);
        }

        private static bool HasValidFormat(XElement document)
        {
            // Access the root Configuration element
            foreach (var child in document.Elements())
            {
                if (child.Name == "Metadata")
                    foreach (var child2 in child.Elements())
                    {
                        if (child2.Name == "Format")
                            return child2.Value is "ConfigAdapter.Xml.v4";
                    }
            }
            return false;
        }

        private static ConfigAdapterSection PopulateGlobalSettings(XElement document)
        {
            var result = new ConfigAdapterSection()
            {
                Name = "_Global",
                Settings = new List<ConfigAdapterSetting>()
            };

            var contentNode = FindContentNode(document);
            foreach (var child in contentNode.Elements())
            {
                if (child.Name == "Setting")
                    result.Settings.Add(FormSetting(child));
            }

            return result;
        }

        private static XElement FindContentNode(XElement document)
        {
            foreach (var child in document.Elements())
            {
                if (child.Name == "Content")
                    return child;
            }

            throw new IncorrectFormatException();
        }

        private static ConfigAdapterSetting FormSetting(XElement node)
        {
            if (node.Elements().Any(e => e.Name == "Values"))
            {
                // Node contains an ArrayValue
                var name = node.Elements().First().Value;
                var values = node.Elements().Skip(1).First().Elements();
                string? comment = node.Elements().Skip(2).First().Value;

                if (string.IsNullOrWhiteSpace(comment))
                    comment = null;

                var array = new List<string>();

                foreach (var value in values)
                    array.Add(value.Value);

                return new ConfigAdapterSetting()
                {
                    Name = name ?? "[NO NAME]",
                    Value = array,
                    Comment = comment
                };
            }
            else if (node.Elements().Any(e => e.Name == "Value"))
            {
                // Node contains a StringValue
                var name = node.Elements().First().Value;
                var value = node.Elements().Skip(1).First().Value;
                string? comment = node.Elements().Skip(2).First().Value;

                if (string.IsNullOrWhiteSpace(comment))
                    comment = null;

                return new ConfigAdapterSetting()
                {
                    Name = name ?? "[NO NAME]",
                    Value = value ?? "[NO VALUE]",
                    Comment = comment
                };
            }
            else
            {
                // Node contains an EmptyValue
                var name = node.Elements().First().Value;
                string? comment = node.Elements().Skip(1).First().Value;

                if (string.IsNullOrWhiteSpace(comment))
                    comment = null;

                return new ConfigAdapterSetting()
                {
                    Name = name ?? "[NO NAME]",
                    Value = new ConfigAdapterValue.EmptyValue(),
                    Comment = comment
                };
            }
        }

        private static ConfigAdapterSection PopulateSection(XElement root)
        {
            var currentSection = new ConfigAdapterSection()
            {
                Settings = new List<ConfigAdapterSetting>(),
                Nested = new List<ConfigAdapterSection>()
            };

            foreach (var child in root.Elements())
            {
                if (child.Name == "Name")
                    currentSection.Name = child.Value;

                // Fill local settings
                if (child.Name == "Setting")
                    currentSection.Settings.Add(FormSetting(child));

                // Fill nested sections
                if (child.Name == "Section")
                    currentSection.Nested.Add(PopulateSection(child));
            }

            return currentSection;
        }

        private static void CreateFramework(XElement document)
        {
            var format = new XElement("Format", "ConfigAdapter.Xml.v4");
            var meta = new XElement("Metadata");
            meta.Add(format);

            var content = new XElement("Content");

            document.Add(meta);
            document.Add(content);
        }

        private static void PersistGlobalSettings(XElement document, ConfigAdapterFile file)
        {
            var content = document.Elements().Single(e => e.Name == "Content");
            var globalSettings = file.Sections.Single(s => s.Name == "_Global").Settings;

            foreach (var globalSetting in globalSettings)
                content.Add(FormSetting(globalSetting));
        }

        private static void PersistLocalSection(XElement root, ConfigAdapterSection section)
        {
            if (section.Name is "_Global")
                return;

            var node = new XElement("Section");
            node.Add(new XElement("Name", section.Name));

            foreach (var setting in section.Settings)
                node.Add(FormSetting(setting));

            foreach (var nested in section.Nested)
                PersistLocalSection(node, nested);

            root.Add(node);
        }

        private static XElement FormSetting(ConfigAdapterSetting s)
        {
            var setting = new XElement("Setting");
            setting.Add(new XElement("Key", s.Name));

            switch (s.Value.TypeHint)
            {
                case "string":
                    FormStringValue(setting, s.Value);
                    break;
                case "array":
                    FormArrayValue(setting, s.Value);
                    break;
                default:
                    FormEmptyValue(setting);
                    break;
            };
            setting.Add(new XElement("Comment", s.Comment ?? string.Empty));

            return setting;
        }

        private static void FormArrayValue(XElement root, ConfigAdapterValue values)
        {
            var container = new XElement("Values");

            foreach (var value in (List<string>)values)
            {
                container.Add(new XElement("Value", value));
            }

            root.Add(container);
        }

        private static void FormStringValue(XElement root, ConfigAdapterValue value)
        {
            root.Add(new XElement("Value", (string)value));
        }

        private static void FormEmptyValue(XElement root)
        {
            // Intentionally left empty
        }
    }
}
