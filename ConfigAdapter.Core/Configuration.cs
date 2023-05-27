using ConfigAdapter.Core;
using ConfigAdapter.Core.Exceptions;
using ConfigAdapter.Core.Model;
using System.Reflection;

namespace ConfigAdapter;

public static class Configuration
{
    private static readonly IDictionary<string, Type> Formats = new Dictionary<string, Type>();

    public static void RegisterProvider(string fileFormat, Type provider)
    {
        var normalized = fileFormat.ToLowerInvariant();

        // Only support actual configuration providers
        if (!typeof(IConfigurationProvider).IsAssignableFrom(provider))
            return;

        if (Formats.ContainsKey(normalized))
            Formats.Remove(normalized);

        Formats.Add(normalized, provider);
    }

    public static bool HasProvider(string fileFormat) => Formats.ContainsKey(fileFormat.ToLowerInvariant());

    public static IConfigurationProvider From(string filePath)
    {
        var fileFormat = new FileInfo(filePath).Extension.Replace(".", "");

        if (!Formats.ContainsKey(fileFormat.ToLowerInvariant()))
            throw new NoProviderAvailableException();

        var provider = Formats[fileFormat.ToLowerInvariant()];
        var instance = Activator.CreateInstance(provider) as IConfigurationProvider;
#pragma warning disable CS8602 // Formats is private, and type is enforced on addition
        instance.Open(filePath);
#pragma warning restore CS8602

        return instance;
    }

    public static IConfigurationProvider Migrate(IConfigurationProvider source, string path)
    {
        var fileFormat = new FileInfo(path).Extension.Replace(".", "");
        if (!HasProvider(fileFormat))
            throw new NoProviderAvailableException();

        var target = From(path);
        var targetFile = target.GetType().GetRuntimeFields()
            .FirstOrDefault(f => f.CustomAttributes.Any(a => a.AttributeType == typeof(ConfigurationClassModelAttribute)));
        var sourceFile = source.GetType().GetRuntimeFields()
            .FirstOrDefault(f => f.CustomAttributes.Any(a => a.AttributeType == typeof(ConfigurationClassModelAttribute)));

        targetFile.SetValue(target, sourceFile.GetValue(source));
        return target;
    }

    static Configuration()
    {
        var dlls = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "ConfigAdapter*.dll");

        foreach (var dll in dlls )
        {
            var assembly = Assembly.LoadFile(dll);

            var providers = assembly.GetTypes().Where(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(ConfigurationProviderAttribute)));
            foreach (var provider in providers)
            {
                var initializer = provider.GetRuntimeMethods().FirstOrDefault(m => m.CustomAttributes.Any(a => a.AttributeType == typeof(ConfigurationInitializerAttribute)));
                initializer?.Invoke(null, null);
            }
        }
    }
}
