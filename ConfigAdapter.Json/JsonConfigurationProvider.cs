using ConfigAdapter.Core;
using ConfigAdapter.Core.Exceptions;
using ConfigAdapter.Core.Model;
using ConfigAdapter.Json;

namespace ConfigAdapter;

[ConfigurationProvider]
public class JsonConfigurationProvider: IConfigurationProvider
{
    [ConfigurationClassModel]
    private ConfigAdapterFile? _file;
    internal string _path = string.Empty;

    public void Open(string path)
    {
        _path = path;

        if (File.Exists(_path))
            _file = JsonFileParser.ParseFile(_path);
        else
            _file = new()
            {
                FileExtension = "JSON",
                FileName = new FileInfo(_path).Name,
                Sections = new List<ConfigAdapterSection>()
            };
    }

    public IEnumerable<ConfigAdapterSetting> Enumerate(string? section = null)
    {
        if (_file is null)
            throw new InconsistentStateException();

        return _file.Enumerate(section);
    }

    public ConfigAdapterSetting? Retrieve(string key)
    {
        if (_file is null)
            throw new InconsistentStateException();

        return _file.NavigateTo(key);
    }

    public void Store(ConfigAdapterSetting setting)
    {
        Store(setting.Name, setting.Value, setting.Comment);
    }

    public void Store(string key, ConfigAdapterValue value, string? comment = null)
    {
        if (_file is null)
            throw new InconsistentStateException();

        _file.Insert(key, value, comment);
    }

    public void Delete(string key)
    {
        if (_file is null)
            throw new InconsistentStateException();

        _file.Delete(key);
    }

    public void Persist()
    {
        if (_file is null)
            throw new InconsistentStateException();

        JsonFileParser.PersistFile(_file, _path);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    [ConfigurationInitializer]
    public static void RegisterProvider()
    {
        Configuration.RegisterProvider("JSON", typeof(JsonConfigurationProvider));
        Configuration.RegisterProvider("HJSON", typeof(JsonConfigurationProvider));
    }
}
