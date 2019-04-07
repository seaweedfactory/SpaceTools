using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceTools.Tools.Crawler
{
    /// <summary>
    /// Holds a list of connections from a profile and if they have been processed.
    /// </summary>
    public class CompleteFile
    {
        /// <summary>
        /// Name of the source profile.
        /// </summary>
        public String UserName { get; set; }

        /// <summary>
        /// Have all connections been processed?
        /// </summary>
        public bool AllConnectionsParsed { get; set; }

        /// <summary>
        /// List of connections to parse.
        /// </summary>
        public List<ConnectionParsedEntry> ConnectionsParsed { get; set; } = new List<ConnectionParsedEntry>();

        /// <summary>
        /// Holds a list of connections from a profile and if they have been processed.
        /// </summary>
        /// <param name="userName">Name of the source profile.</param>
        public CompleteFile(String userName)
        {
            UserName = userName;
        }
    }

    /// <summary>
    /// Parser list entry.
    /// </summary>
    public class ConnectionParsedEntry
    {
        /// <summary>
        /// Name of the profile to parse.
        /// </summary>
        public String UserName { get; set; }

        /// <summary>
        /// Has the profile been parsed?
        /// </summary>
        public bool Parsed { get; set; }

        /// <summary>
        /// Date the profile was parsed, if available.
        /// </summary>
        public DateTime? DateParsed { get; set; }
    }
}
