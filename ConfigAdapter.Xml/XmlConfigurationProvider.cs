using ConfigAdapter.Core;
using ConfigAdapter.Core.Exceptions;
using ConfigAdapter.Core.Model;
using ConfigAdapter.Xml;

namespace ConfigAdapter;

[ConfigurationProvider]
public class XmlConfigurationProvider : IConfigurationProvider
{
    private ConfigAdapterFile? _file;
    internal string _path = string.Empty;

    public void Open(string path)
    {
        _path = path;

        if (File.Exists(_path))
            _file = XmlFileParser.ParseFile(_path);
        else
            _file = new()
            {
                FileExtension = "XML",
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

    public void Persist()
    {
        if (_file is null)
            throw new InconsistentStateException();

        XmlFileParser.PersistFile(_file, _path);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    [ConfigurationInitializer]
    public static void RegisterProvider()
    {
        Configuration.RegisterProvider("XML", typeof(XmlConfigurationProvider));
    }
}
