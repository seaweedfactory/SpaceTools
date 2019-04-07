using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceTools.Utility
{
    /// <summary>
    /// Logging interface.
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// Log the line to file.
        /// </summary>
        /// <param name="message">Message to log.</param>
        void Log(String message);
    }
}
