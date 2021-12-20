using ConfigAdapter.Core.Exceptions;
using ConfigAdapter.Core.Model;
using IniParser.Model;
using IniParser.Parser;

namespace ConfigAdapter.Ini;

internal class IniFileParser
{
    internal static ConfigAdapterFile ParseFile(string path)
    {
        var result = new ConfigAdapterFile()
        {
            FileExtension = "INI",
            FileName = new FileInfo(path).Name,
            Sections = new List<ConfigAdapterSection>()
        };

        var allText = File.ReadAllText(path);
        var document = new IniDataParser().Parse(allText);

        // Check metadata for valid format
        if (!HasValidFormat(document))
            throw new IncorrectFormatException();

        // Fill global settings
        result.Sections.Add(PopulateGlobalSettings(document));

        // Fill sections
        //var contentNode = FindContentNode(document);
        //foreach (var child in contentNode.Elements())
        //{
        //    if (child.Name == "Section")
        //        result.Sections.Add(PopulateSection(child));
        //}

        return result;
    }

    internal static void PersistFile(ConfigAdapterFile root, string path)
    {
        //var document = new XElement("Configuration");
        //CreateFramework(document);

        //PersistGlobalSettings(document, root);

        //foreach (var section in root.Sections)
        //    PersistLocalSection(document.Elements().Single(e => e.Name == "Content"), section);

        //document.Save(path);
    }

    private static bool HasValidFormat(IniData document)
    {
        foreach (var section in document.Sections)
            if (section.SectionName is "Metadata")
                foreach (var key in section.Keys)
                    if (key.KeyName is "Format")
                        return key.Value is "ConfigAdapter.Ini.v4";
        
        return false;
    }

    private static ConfigAdapterSection PopulateGlobalSettings(IniData document)
    {
        var result = new ConfigAdapterSection()
        {
            Name = "_Global",
            Settings = new List<ConfigAdapterSetting>()
        };

        foreach (var section in document.Sections)
            if (section.SectionName is "_Global")
            {
                foreach (var key in section.Keys.Where(k => k.KeyName.EndsWith(".Comment")))
                {
                    result.Settings.Add(FormSetting(section, key));
                }
                break;
            }

        return result;
    }

    private static ConfigAdapterSetting FormSetting(SectionData section, KeyData key)
    {
        var keyRoot = key.KeyName.Split('.').First();

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
}
