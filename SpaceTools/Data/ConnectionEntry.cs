using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceTools.Data
{
    /// <summary>
    /// Describes a connection.
    /// </summary>
    public class ConnectionEntry
    {
        /// <summary>
        /// URL of connection.
        /// </summary>
        public String UserURL { get; set; }

        /// <summary>
        /// Profile name of connection.
        /// </summary>
        public String UserName { get; set; }

        /// <summary>
        /// Name as it appears on the profile.
        /// </summary>
        public String PersonalName { get; set; }

        /// <summary>
        /// URL to profile picture thumbnail.
        /// </summary>
        public String ThumbnailURL { get; set; }

        /// <summary>
        /// Direction of connection, in or out.
        /// </summary>
        public ConnectionDirection Direction { get; set; }

        /// <summary>
        /// ID of profile.
        /// </summary>
        public String ProfileID { get; set; }

        /// <summary>
        /// Artist ID of profile, not present on all profiles.
        /// </summary>
        public String ArtistID { get; set; }

        public override string ToString()
        {
            return String.Format("{2}: {0}, {1}",PersonalName,UserURL,Direction.ToString());
        }
    }

    /// <summary>
    /// Direction of profile connection.
    /// </summary>
    public enum ConnectionDirection
    {
        /// <summary>
        /// Connection direction is unknown.
        /// </summary>
        [Description("Unknown")]
        Unknown = 0,

        /// <summary>
        /// People they are connected to.
        /// </summary>
        [Description("Out")]
        Out = 1,

        /// <summary>
        /// People connected to them.
        /// </summary>
        [Description("In")]
        In = 2,

    }
}
