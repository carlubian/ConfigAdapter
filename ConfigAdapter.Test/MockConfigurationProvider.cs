using ConfigAdapter.Core;
using ConfigAdapter.Core.Model;
using System.Collections.Generic;

namespace ConfigAdapter.Test;

internal class MockConfigurationProvider : IConfigurationProvider
{
    public void Open(string path)
    {
    }

    public IEnumerable<ConfigAdapterSetting> Enumerate(string? section = null)
    {
        yield return new()
        {
            Name = "String key",
            Comment = "Test configuration setting",
            Value = new ConfigAdapterValue.StringValue("Test configuration value")
        };
        yield return new()
        {
            Name = "Array key",
            Comment = "Test configuration setting",
            Value = new ConfigAdapterValue.ArrayValue(new List<string>(new string[] { "Value 1", "Value 2" }))
        };
    }

    public ConfigAdapterSetting Retrieve(string key)
    {
        return new()
        {
            Name = key,
            Comment = "Test configuration setting",
            Value = new ConfigAdapterValue.StringValue("Test configuration value")
        };
    }

    public void Store(ConfigAdapterSetting setting)
    {
    }

    public void Store(string key, ConfigAdapterValue value, string? comment = null)
    {
    }

    public void Dispose()
    {
    }

    public void Persist()
    {
    }

    public void Delete(string key)
    {
    }
}
