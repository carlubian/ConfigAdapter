using System;

[assembly: CLSCompliant(true)]
namespace ConfigAdapter.Xml
{
    public static class XmlConfig
    {
        public static Config From(string file)
        {
            return new Config(new XmlFileAdapter(file));
        }
    }
}
