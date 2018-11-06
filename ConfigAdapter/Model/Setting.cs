using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigAdapter.Model
{
    /// <summary>
    /// Temporarily stores configuration settings
    /// during a transference.
    /// </summary>
    internal struct Setting
    {
        /// <summary>
        /// Key name, including category if present.
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Setting value.
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Setting comment if present.
        /// </summary>
        public string Comment { get; set; }

        public Setting(string key, string value, string comment)
        {
            Key = key;
            Value = value;
            Comment = comment;
        }
    }
}
