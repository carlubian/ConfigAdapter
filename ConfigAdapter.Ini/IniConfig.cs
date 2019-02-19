using System;

namespace ConfigAdapter.Ini
{
    public static class IniConfig
    {
        public static Config From(string file)
        {
            return new Config(new IniFileAdapter(file));
        }
    }
}
