using ConfigAdapter.Core.Exceptions;
using ConfigAdapter.Core.Model;
using System.Text.Json;

namespace ConfigAdapter.Json;

internal static class JsonFileParser
{
    internal static ConfigAdapterFile ParseFile(string path)
    {
        var result = new ConfigAdapterFile()
        {
            FileExtension = "JSON",
            FileName = new FileInfo(path).Name,
            Sections = new List<ConfigAdapterSection>()
        };

        var content = File.ReadAllText(path);
        var document = JsonDocument.Parse(content);

        // Check metadata for valid format
        if (!HasValidFormat(document))
            throw new IncorrectFormatException();

        // Fill global settings
        result.Sections.Add(PopulateGlobalSettings(document));

        // Fill sections
        var sectionsNode = FindSectionsNode(document.RootElement);
        foreach (var section in sectionsNode.Value.EnumerateObject())
        {
            result.Sections.Add(PopulateSection(section));
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
        var nested = new Dictionary<string, object?>();
        foreach (var subsection in root.Sections)
        {
            if (subsection.Name is "_Global")
                continue;

            var subsectionValues = new Dictionary<string, object?>();
            PersistLocalSection(subsectionValues, subsection);
            nested.Add(subsection.Name, subsectionValues);
        }

        if (nested.Any())
            content.Add("Sections", nested);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var newFile = JsonSerializer.Serialize(document, options);
        File.WriteAllText(path, newFile);
    }
#pragma warning restore CS8602

    private static bool HasValidFormat(JsonDocument document)
    {
        foreach (var node in document.RootElement.EnumerateObject())
            if (node.Name is "Configuration")
                foreach (var elem in node.Value.EnumerateObject())
                    if (elem.Name is "Metadata")
                        foreach (var child in elem.Value.EnumerateObject())
                            if (child.Name is "Format")
                                return child.Value.ToString() == "ConfigAdapter.Json.v4";
        
        return false;
    }

    private static ConfigAdapterSection PopulateGlobalSettings(JsonDocument document)
    {
        var result = new ConfigAdapterSection()
        {
            Name = "_Global",
            Settings = new List<ConfigAdapterSetting>()
        };

        var contentNode = FindContentNode(document);
        foreach (var node in contentNode.Value.EnumerateObject())
        {
            if (node.Name != "Sections")
                result.Settings.Add(FormSetting(node));
        }

        return result;
    }

    private static JsonProperty FindContentNode(JsonDocument document)
    {
        foreach (var node in document.RootElement.EnumerateObject())
            if (node.Name is "Configuration")
                foreach (var elem in node.Value.EnumerateObject())
                    if (elem.Name is "Content")
                        return elem;

        throw new IncorrectFormatException();
    }

    private static JsonProperty FindSectionsNode(JsonElement root)
    {
        foreach (var node in root.EnumerateObject())
            if (node.Name is "Configuration")
                foreach (var elem in node.Value.EnumerateObject())
                    if (elem.Name is "Content")
                        foreach (var child in elem.Value.EnumerateObject())
                            if (child.Name is "Sections")
                                return child;

        throw new IncorrectFormatException();
    }

    private static ConfigAdapterSetting FormSetting(JsonProperty node)
    {
        if (node.Value.EnumerateObject().Any(e => e.Name == "Values"))
        {
            // Node contains an ArrayValue
            var name = node.Name;
            var values = node.Value.EnumerateObject()
                .First(n => n.Name is "Values").Value;
            var comment = node.Value.EnumerateObject()
                .First(n => n.Name is "Comment").Value.ToString();

            if (string.IsNullOrWhiteSpace(comment))
                comment = null;

            var array = new List<string>();

            foreach (var value in values.EnumerateArray())
                array.Add(value.GetString() ?? "[NO VALUE]");

            return new ConfigAdapterSetting()
            {
                Name = name,
                Value = array,
                Comment = comment
            };
        }
        else if (node.Value.EnumerateObject().Any(e => e.Name == "Value"))
        {
            // Node contains a StringValue
            var name = node.Name;
            var value = node.Value.EnumerateObject()
                .First(n => n.Name is "Value").Value.ToString();
            var comment = node.Value.EnumerateObject()
                .First(n => n.Name is "Comment").Value.ToString();

            if (string.IsNullOrWhiteSpace(comment))
                comment = null;

            return new ConfigAdapterSetting()
            {
                Name = name,
                Value = value ?? "[NO VALUE]",
                Comment = comment
            };
        }
        else
        {
            // Node contains an EmptyValue
            var name = node.Name;
            var comment = node.Value.EnumerateObject()
                .First(n => n.Name is "Comment").Value.ToString();

            if (string.IsNullOrWhiteSpace(comment))
                comment = null;

            return new ConfigAdapterSetting()
            {
                Name = name,
                Value = new ConfigAdapterValue.EmptyValue(),
                Comment = comment
            };
        }
    }

    private static ConfigAdapterSection PopulateSection(JsonProperty root)
    {
        var currentSection = new ConfigAdapterSection()
        {
            Name = root.Name,
            Settings = new List<ConfigAdapterSetting>(),
            Nested = new List<ConfigAdapterSection>()
        };

        foreach (var child in root.Value.EnumerateObject())
        {
            // Fill nested sections
            if (child.Name is "Sections")
            {
                foreach (var section in child.Value.EnumerateObject())
                    currentSection.Nested.Add(PopulateSection(section));
            }
            // Fill local settings
            else
                currentSection.Settings.Add(FormSetting(child));
        }

        return currentSection;
    }

    private static void CreateFramework(IDictionary<string, object> document)
    {
        var configuration = new Dictionary<string, object>()
        {
            { "Metadata", new Dictionary<string, object>
                {
                    { "Format", "ConfigAdapter.Json.v4" }
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
        foreach (var setting in section.Settings)
            root.Add(setting.Name, FormSetting(setting));

        var nested = new Dictionary<string, object?>();
        foreach (var subsection in section.Nested)
        {
            var subsectionValues = new Dictionary<string, object?>();
            PersistLocalSection(subsectionValues, subsection);
            nested.Add(subsection.Name, subsectionValues);
        }

        if (nested.Any())
            root.Add("Sections", nested);
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
