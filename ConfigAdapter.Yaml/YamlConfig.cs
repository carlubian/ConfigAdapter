using System;

[assembly: CLSCompliant(true)]
namespace ConfigAdapter.Yaml
{
    public static class YamlConfig
    {
        public static Config From(string file)
        {
            return new Config(new YamlFileAdapter(file));
        }
    }
}
