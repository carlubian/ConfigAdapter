using ConfigAdapter.Core.Model;

namespace ConfigAdapter.Core;

public interface IConfigurationProvider : IDisposable
{
    void Open(string path);
    void Store(ConfigAdapterSetting setting);
    void Store(string key, ConfigAdapterValue value, string? comment = null);
    ConfigAdapterSetting? Retrieve(string key);
    IEnumerable<ConfigAdapterSetting> Enumerate(string? section = null);
    void Persist();
}
