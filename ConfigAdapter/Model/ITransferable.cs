using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigAdapter.Model
{
    /// <summary>
    /// Defines the necessary methods to transfer
    /// settings from one file to another.
    /// </summary>
    public interface ITransferable
    {
        /// <summary>
        /// Returns a list with all the settings in the file.
        /// </summary>
        /// <returns></returns>
        IList<Setting> ReadAll();

        /// <summary>
        /// Writes all the settings in the list into a file.
        /// </summary>
        /// <param name="source"></param>
        void WriteAll(IList<Setting> source);
    }
}
