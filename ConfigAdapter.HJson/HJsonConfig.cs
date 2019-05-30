using System;

[assembly: CLSCompliant(true)]
namespace ConfigAdapter.HJson
{
    public static class HJsonConfig
    {
        public static Config From(string file)
        {
            return new Config(new HJsonFileAdapter(file));
        }
    }
}
