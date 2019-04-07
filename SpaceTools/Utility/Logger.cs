using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceTools.Utility
{
    /// <summary>
    /// Log messages.
    /// </summary>
    public class Logger : ILog, IDisposable
    {
        /// <summary>
        /// Log file writer.
        /// </summary>
        private StreamWriter LogFile { get; set; }

        /// <summary>
        /// Set up logger to append to the provided log file.
        /// </summary>
        /// <param name="storeDirectory">Directory containing log file.</param>
        /// <param name="logPrefix">Log file prefile.</param>
        public Logger(String storeDirectory, String logPrefix)
        {
            try
            {
                LogFile = File.AppendText(Path.Combine(storeDirectory, String.Format("{0}_{1}.log", logPrefix, DateTime.Now.ToString("yyyy_MM_dd"))));
            }
            catch (Exception e)
            {
                LogFile = null;
            }
        }

        /// <summary>
        /// Log a message to the log file.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public void Log(String message)
        {
            String formattedMessage = String.Format("{0}: {1}", DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss"), message);
            if (LogFile != null)
            {
                LogFile.WriteLine(formattedMessage);
                LogFile.Flush();
            }
            System.Console.WriteLine(formattedMessage);
        }

        public void Dispose()
        {
            if (LogFile != null)
            {
                LogFile.Flush();
                LogFile.Close();
            }
        }
    }
}
