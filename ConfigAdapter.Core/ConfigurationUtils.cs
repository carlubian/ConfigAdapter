using ConfigAdapter.Core;
using ConfigAdapter.Core.Model;

namespace ConfigAdapter;

public static class ConfigurationUtils
{
    // ---------- Extension methods exposed to the users ---------- \\
    public static string? GetValue(this IConfigurationProvider config, string settingKey)
    {
        if (config is null || string.IsNullOrWhiteSpace(settingKey))
            return default;

        var setting = config.Retrieve(settingKey);
        if (setting is null)
            return default;

        return setting.Value;
    }

    public static List<string> GetValues(this IConfigurationProvider config, string settingKey)
    {
        if (config is null || string.IsNullOrWhiteSpace(settingKey))
            return new List<string>();

        var setting = config.Retrieve(settingKey);
        if (setting is null)
            return new List<string>();

        return setting.Value;
    }

    // ---------- Extension methods used internally by the library ---------- \\
    public static ConfigAdapterSetting? NavigateTo(this ConfigAdapterFile file, string settingKey)
    {
        if (settingKey.Contains(":"))
        {
            // Local setting
            var (sectionName, remaining) = IterateSettingKey(settingKey);
            var section = file.Sections.FirstOrDefault(s => s.Name == sectionName);

            return RecursiveRetrieval(section, remaining);
        }
        else
        {
            // Global setting
            var global = file.Sections.FirstOrDefault(s => s.Name is "_Global");
            return RecursiveRetrieval(global, settingKey);
        }
    }

    public static void Insert(this ConfigAdapterFile file, string key, ConfigAdapterValue value, string? comment)
    {
        if (key.Contains(":"))
        {
            // Local setting
            var (sectionName, remaining) = IterateSettingKey(key);
            EnsureSectionExists(file.Sections, sectionName);
            var section = file.Sections.Single(s => s.Name == sectionName);

            RecursiveInsertion(section, remaining, value, comment);
        }
        else
        {
            // Global setting
            EnsureSectionExists(file.Sections, "_Global");
            var global = file.Sections.Single(s => s.Name is "_Global");
            RecursiveInsertion(global, key, value, comment);
        }
    }

    public static IEnumerable<ConfigAdapterSetting> Enumerate(this ConfigAdapterFile file, string? sectionName)
    {
        if (sectionName is null)
            return file.Sections.Single(s => s.Name is "_Global").Settings;

        var (sctName, remaining) = IterateSettingKey(sectionName);
        var section = file.Sections.FirstOrDefault(s => s.Name == sctName);
        return RecursiveEnumeration(section, remaining);
    }

    public static void Delete(this ConfigAdapterFile file, string key)
    {
        if (key.Contains(':'))
        {
            // Nested file or section
            var (sctName, remaining) = IterateSettingKey(key);
            var section = file.Sections.FirstOrDefault(s => s.Name == sctName);

            RecursiveDeletion(section, remaining);
            if (section is not null && section.Settings.Count is 0 && section.Nested.Count is 0)
                file.Sections.Remove(section);
        }
        else
        {
            // Local file or section
            if (file.Sections.Any(s => s.Name == key))
            {
                var toRemove = file.Sections.First(s => s.Name == key);
                file.Sections.Remove(toRemove);
            }
            else
            {
                var global = file.Sections.Single(s => s.Name is "_Global");
                var toDelete = global.Settings.FirstOrDefault(s => s.Name == key);

                if (toDelete is not null)
                    global.Settings.Remove(toDelete);
            }
        }
    }

    // ---------- Private utility methods ---------- \\

    private static ConfigAdapterSetting? RecursiveRetrieval(ConfigAdapterSection? s, string key)
    {
        if (s is null)
            return null;

        if (key.Contains(":"))
        {
            // Setting is at least one section deeper
            var (sectionName, remaining) = IterateSettingKey(key);
            var section = s.Nested.FirstOrDefault(s => s.Name == sectionName);

            return RecursiveRetrieval(section, remaining);
        }
        else
        {
            // Setting is in this section
            return s.Settings.FirstOrDefault(s => s.Name == key);
        }
    }

    private static void RecursiveInsertion(ConfigAdapterSection s, string key, ConfigAdapterValue value, string? comment)
    {
        if (key.Contains(":"))
        {
            // Setting is nested further
            var (sectionName, remaining) = IterateSettingKey(key);
            EnsureSectionExists(s.Nested, sectionName);
            var section = s.Nested.Single(s => s.Name == sectionName);

            RecursiveInsertion(section, remaining, value, comment);
        }
        else
        {
            // Insert in this section
            s.Settings.Add(new()
            {
                Name = key,
                Value = value,
                Comment = comment
            });
        }
    }

    private static IEnumerable<ConfigAdapterSetting> RecursiveEnumeration(ConfigAdapterSection? section, string sectionName)
    {
        if (section is null)
            return Enumerable.Empty<ConfigAdapterSetting>();

        if (sectionName.Contains(":"))
        {
            // Targets is at least two section deep
            var (sctName, remaining) = IterateSettingKey(sectionName);
            var s = section.Nested.FirstOrDefault(s => s.Name == sctName);

            return RecursiveEnumeration(s, remaining);
        }
        else if (string.IsNullOrEmpty(sectionName))
        {
            // Target is the current section
            return section.Settings;
        }
        else
        {
            // Target is just one section deep
            var s = section.Nested.FirstOrDefault(s => s.Name == sectionName);
            return RecursiveEnumeration(s, string.Empty);
        }
    }

    private static void RecursiveDeletion(ConfigAdapterSection? section, string sectionName)
    {
        if (section is null)
            return;

        if (sectionName.Contains(':'))
        {
            // Nested file or section
            var (sctName, remaining) = IterateSettingKey(sectionName);
            var sct = section.Nested.FirstOrDefault(s => s.Name == sctName);

            RecursiveDeletion(sct, remaining);
            if (sct is not null && sct.Settings.Count is 0 && sct.Nested.Count is 0)
                section.Nested.Remove(sct);
        }
        else
        {
            // Local file or section
            if (section.Nested.Any(s => s.Name == sectionName))
            {
                var toRemove = section.Nested.First(s => s.Name == sectionName);
                section.Nested.Remove(toRemove);
            }
            else
            {
                var toDelete = section.Settings.FirstOrDefault(s => s.Name == sectionName);

                if (toDelete is not null)
                    section.Settings.Remove(toDelete);
            }
        }
    }

    private static (string sectionName, string remaining) IterateSettingKey(string key)
    {
        var fragments = key.Split(':');
        var remaining = string.Join(':', fragments.Skip(1));

        return (fragments[0], remaining);
    }

    private static void EnsureSectionExists(IList<ConfigAdapterSection> container, string sectionName)
    {
        foreach (var subsection in container)
            if (subsection.Name == sectionName)
                return;

        container.Add(new()
        {
            Name = sectionName,
            Settings = new List<ConfigAdapterSetting>(),
            Nested = new List<ConfigAdapterSection>()
        });
    }
}
