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
        foreach (var child in document.Sections)
        {
            if (child.SectionName is not "_Global" && child.SectionName is not "Metadata")
                PopulateSection(result, child);
        }

        return result;
    }

    internal static void PersistFile(ConfigAdapterFile root, string path)
    {
        var document = new IniData();
        CreateFramework(document);

        PersistGlobalSettings(document, root);

        foreach (var section in root.Sections)
            PersistLocalSection(document, section);

        var newFile = document.ToString();
        File.WriteAllText(path, newFile);
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
        var keyRoot = string.Join('.', key.KeyName.Split('.').Reverse().Skip(1).Reverse());

        if (section.Keys.Any(k => k.KeyName.StartsWith($"{keyRoot}.Values")))
        {
            // Node contains an ArrayValue
            var name = keyRoot;
            string? comment = section.Keys.First(k => k.KeyName == $"{keyRoot}.Comment").Value;

            if (string.IsNullOrWhiteSpace(comment))
                comment = null;

            var array = new List<string>();

            for (int i = 0; i < int.MaxValue; i++)
            {
                if (section.Keys.Any(k => k.KeyName.StartsWith($"{keyRoot}.Values.{i}")))
                {
                    var value = section.Keys.First(k => k.KeyName.StartsWith($"{keyRoot}.Values.{i}")).Value;
                    array.Add(value);
                }
                else
                    break;
            }

            return new ConfigAdapterSetting()
            {
                Name = name ?? "[NO NAME]",
                Value = array,
                Comment = comment
            };
        }
        else if (section.Keys.Any(k => k.KeyName == $"{keyRoot}.Value"))
        {
            // Node contains a StringValue
            var name = keyRoot;
            var value = section.Keys.First(k => k.KeyName == $"{keyRoot}.Value").Value;
            string? comment = section.Keys.First(k => k.KeyName == $"{keyRoot}.Comment").Value;

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
            var name = keyRoot;
            string? comment = section.Keys.First(k => k.KeyName == $"{keyRoot}.Comment").Value;

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

    private static void PopulateSection(ConfigAdapterFile file, SectionData section)
    {
        /**
         * Hacky way to populate the model tree:
         * No need to recurse and split the sections, as that logic
         * is already implemented in the utility "insert" method.
         * 
         * It will create all the hierarchy and link sections together.
         */

        foreach (var key in section.Keys.Where(k => k.KeyName.EndsWith(".Comment")))
        {
            var setting = FormSetting(section, key);
            file.Insert($"{section.SectionName}:{setting.Name}", setting.Value, setting.Comment);
        }
    }

    private static void CreateFramework(IniData document)
    {
        document.Sections.AddSection("Metadata");
        document.Sections.Single().Keys.AddKey("Format", "ConfigAdapter.Ini.v4");
    }

    private static void PersistGlobalSettings(IniData document, ConfigAdapterFile file)
    {
        var globalSettings = file.Sections.Single(s => s.Name == "_Global").Settings;
        document.Sections.AddSection("_Global");
        var section = document.Sections.Single(s => s.SectionName is "_Global");

        foreach (var globalSetting in globalSettings)
            FormSetting(section, globalSetting);
    }

    private static void PersistLocalSection(IniData document, ConfigAdapterSection section)
    {
        if (section.Name is "_Global" || section.Name is "Metadata")
            return;

        document.Sections.AddSection(section.Name);
        var model = document.Sections.Single(s => s.SectionName == section.Name);

        foreach (var setting in section.Settings)
            FormSetting(model, setting);

        foreach (var nested in section.Nested)
        {
            nested.Name = $"{section.Name}:{nested.Name}";
            PersistLocalSection(document, nested);
        }
    }

    private static void FormSetting(SectionData section, ConfigAdapterSetting s)
    {
        switch (s.Value.TypeHint)
        {
            case "string":
                FormStringValue(section, s);
                break;
            case "array":
                FormArrayValue(section, s);
                break;
            default:
                FormEmptyValue(section, s);
                break;
        };
    }

    private static void FormArrayValue(SectionData section, ConfigAdapterSetting s)
    {
        for (int i = 0; i < ((List<string>)s.Value).Count; i++)
            section.Keys.AddKey($"{s.Name}.Values.{i}", ((List<string>)s.Value)[i]);

        section.Keys.AddKey($"{s.Name}.Comment", s.Comment);
    }

    private static void FormStringValue(SectionData section, ConfigAdapterSetting s)
    {
        section.Keys.AddKey($"{s.Name}.Value", (string)s.Value);
        section.Keys.AddKey($"{s.Name}.Comment", s.Comment);
    }

    private static void FormEmptyValue(SectionData section, ConfigAdapterSetting s)
    {
        section.Keys.AddKey($"{s.Name}.Comment", s.Comment);
    }
}
