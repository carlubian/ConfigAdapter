using ConfigAdapter.Core.Exceptions;
using ConfigAdapter.Core.Model;
using System.Collections.Immutable;
using YamlDotNet.Serialization;

namespace ConfigAdapter.Yaml;

internal static class YamlFileParser
{
    internal static ConfigAdapterFile ParseFile(string path)
    {
        var result = new ConfigAdapterFile()
        {
            FileExtension = "YAML",
            FileName = new FileInfo(path).Name,
            Sections = new List<ConfigAdapterSection>()
        };

        var deserializer = new Deserializer();
        var content = File.ReadAllText(path);
        var root = deserializer.Deserialize<IDictionary<string, object>>(content);

        // Check metadata for valid format
        if (!HasValidFormat(root))
            throw new IncorrectFormatException();

        // Fill global settings
        result.Sections.Add(PopulateGlobalSettings(root));

        // Fill sections
        var sectionsNode = FindSectionsNode(root);
        foreach (var child in sectionsNode)
        {
            var elements = child as IDictionary<object, object>;
            result.Sections.Add(PopulateSection(elements?.Keys.Single()?.ToString() ?? "[NO NAME]",
                elements?[elements?.Keys.Single() ?? string.Empty] as IDictionary<object, object>
                ?? ImmutableDictionary<object, object>.Empty));
        }

        return result;
    }

#pragma warning disable CS8602
    internal static void PersistFile(ConfigAdapterFile root, string path)
    {
        var document = new Dictionary<string, object>();
        CreateFramework(document);

        PersistGlobalSettings(document, root);

        var configuration = document["Configuration"] as IDictionary<string, object>;
        var content = configuration["Content"] as IDictionary<string, object>;
        var nested = new List<IDictionary<string, object?>>();
        foreach (var subsection in root.Sections)
        {
            if (subsection.Name is "_Global")
                continue;

            var subsectionDict = new Dictionary<string, object?>();
            var subsectionValues = new Dictionary<string, object?>();
            subsectionDict.Add(subsection.Name, subsectionValues);
            PersistLocalSection(subsectionDict, subsection);
            nested.Add(subsectionDict);
        }

        if (nested.Any())
            content.Add("Sections", nested);

        var newFile = new Serializer().Serialize(document);
        File.WriteAllText(path, newFile);
    }
#pragma warning restore CS8602

    private static bool HasValidFormat(IDictionary<string, object> root)
    {
        if (root is null || !root.ContainsKey("Configuration"))
            return false;

        if (root["Configuration"] is not IDictionary<object, object> config || !config.ContainsKey("Metadata"))
            return false;

        if (config["Metadata"] is not IDictionary<object, object> meta || !meta.ContainsKey("Format"))
            return false;

        return meta["Format"] is "ConfigAdapter.Yaml.v4";
    }

    private static ConfigAdapterSection PopulateGlobalSettings(IDictionary<string, object> root)
    {
        var result = new ConfigAdapterSection()
        {
            Name = "_Global",
            Settings = new List<ConfigAdapterSetting>()
        };

        var content = FindContentNode(root);

        foreach (var settingBlock in content)
        {
            if (settingBlock.Key is "Sections")
                continue;

            result.Settings.Add(FormSetting(settingBlock.Key?.ToString() ?? "[NO NAME]", 
                settingBlock.Value as IDictionary<object, object> ?? ImmutableDictionary<object, object>.Empty));
        }

        return result;
    }

    private static IDictionary<object, object> FindContentNode(IDictionary<string, object> root)
    {
        if (root is null || !root.ContainsKey("Configuration"))
            return ImmutableDictionary<object, object>.Empty;

        if (root["Configuration"] is not IDictionary<object, object> config || !config.ContainsKey("Content"))
            return ImmutableDictionary<object, object>.Empty;

        var content = config["Content"] as IDictionary<object, object>;

        return content ?? ImmutableDictionary<object, object>.Empty;
    }

    private static IList<object> FindSectionsNode(IDictionary<string, object> root)
    {
        if (root is null || !root.ContainsKey("Configuration"))
            return Array.Empty<object>();

        if (root["Configuration"] is not IDictionary<object, object> config || !config.ContainsKey("Content"))
            return Array.Empty<object>();

        if (config["Content"] is not IDictionary<object, object> content || !content.ContainsKey("Sections"))
            return Array.Empty<object>();

        var sections = content["Sections"] as IList<object>;

        return sections ?? Array.Empty<object>();
    }

    private static ConfigAdapterSetting FormSetting(string key, IDictionary<object, object> settingBlock)
    {
        if (settingBlock.ContainsKey("Values"))
        {
            // Node contains an ArrayValue
            var array = new List<string>();

            foreach (var entry in settingBlock["Values"] as List<object> ?? new List<object>())
                array.Add(entry?.ToString() ?? "[NO VALUE]");

            return new ConfigAdapterSetting()
            {
                Name = key,
                Value = array,
                Comment = settingBlock["Comment"]?.ToString()
            };
        }
        else if (settingBlock.ContainsKey("Value"))
        {
            // Node contains a StringValue
            return new ConfigAdapterSetting()
            {
                Name = key,
                Value = settingBlock["Value"]?.ToString() ?? "[NO VALUE]",
                Comment = settingBlock["Comment"]?.ToString()
            };
        }
        else
        {
            // Node contains an EmptyValue
            return new ConfigAdapterSetting()
            {
                Name = key,
                Value = new ConfigAdapterValue.EmptyValue(),
                Comment = settingBlock["Comment"]?.ToString()
            };
        }
    }

    private static ConfigAdapterSection PopulateSection(string key, IDictionary<object, object> root)
    {
        var currentSection = new ConfigAdapterSection()
        {
            Name = key,
            Settings = new List<ConfigAdapterSetting>(),
            Nested = new List<ConfigAdapterSection>()
        };

        foreach (var element in root)
        {
            if (element.Key.ToString() is "Sections")
            {
                var subsections = element.Value as IList<object>;
                foreach (var subsection in subsections ?? new List<object>())
                {
                    var nested = subsection as IDictionary<object, object>;

                    currentSection.Nested.Add(PopulateSection(nested?.Keys.Single()?.ToString() ?? "[NO NAME]",
                        nested?[nested?.Keys.Single() ?? string.Empty] as IDictionary<object, object>
                        ?? ImmutableDictionary<object, object>.Empty));
                }
            }
            else
            {
                var settingKeys = element.Value as IDictionary<object, object>;
                currentSection.Settings.Add(FormSetting(element.Key.ToString() ?? "[NO NAME]", 
                    settingKeys ?? ImmutableDictionary<object, object>.Empty));
            }
        }

        return currentSection;
    }

    private static void CreateFramework(IDictionary<string, object> document)
    {
        var configuration = new Dictionary<string, object>()
        {
            { "Metadata", new Dictionary<string, object>
                {
                    { "Format", "ConfigAdapter.Yaml.v4" }
                } 
            },
            { "Content", new Dictionary<string, object>() }
        };
        
        document.Add("Configuration", configuration);
    }

#pragma warning disable CS8602
    private static void PersistGlobalSettings(IDictionary<string, object> document, ConfigAdapterFile file)
    {
        var configuration = document["Configuration"] as IDictionary<string, object>;
        var content = configuration["Content"] as IDictionary<string, object>;
        var globalSettings = file.Sections.Single(s => s.Name == "_Global").Settings;

        foreach (var globalSetting in globalSettings)
            content.Add(globalSetting.Name, FormSetting(globalSetting));
    }
#pragma warning restore CS8602

    private static void PersistLocalSection(IDictionary<string, object?> root, ConfigAdapterSection section)
    {
        var innerSection = root[root.Keys.Single()] as IDictionary<string, object>;

        foreach (var setting in section.Settings)
            innerSection?.Add(setting.Name, FormSetting(setting));

        var nested = new List<IDictionary<string, object?>>();
        foreach (var subsection in section.Nested)
        {
            var subsectionDict = new Dictionary<string, object?>();
            var subsectionValues = new Dictionary<string, object?>();
            subsectionDict.Add(subsection.Name, subsectionValues);
            PersistLocalSection(subsectionDict, subsection);
            nested.Add(subsectionDict);
        }

        if (nested.Any())
            innerSection?.Add("Sections", nested);
    }

    private static IDictionary<string, object?> FormSetting(ConfigAdapterSetting s)
    {
        return s.Value.TypeHint switch
        {
            "string" => FormStringValue(s),
            "array" => FormArrayValue(s),
            _ => FormEmptyValue(s),
        };
        ;
    }

    private static IDictionary<string, object?> FormArrayValue(ConfigAdapterSetting s)
    {
        return new Dictionary<string, object?>()
        {
            { "Values", (List<string>)s.Value },
            { "Comment", s.Comment }
        };
    }

    private static IDictionary<string, object?> FormStringValue(ConfigAdapterSetting s)
    {
        return new Dictionary<string, object?>()
        {
            { "Value", (string)s.Value },
            { "Comment", s.Comment }
        };
    }

    private static IDictionary<string, object?> FormEmptyValue(ConfigAdapterSetting s)
    {
        return new Dictionary<string, object?>()
        {
            { "Comment", s.Comment }
        };
    }
}
