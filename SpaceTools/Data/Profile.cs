using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceTools.Data
{
    /// <summary>
    /// Describes root profile information.
    /// </summary>
    public class Profile
    {
        /// <summary>
        /// URL of profile page.
        /// </summary>
        public String URL { get; set; }

        /// <summary>
        /// URL of profile image.
        /// </summary>
        public String ProfileImageURL { get; set; }

        /// <summary>
        /// URL of profile image thumbnail.
        /// </summary>
        public String ProfileThumbnailImageURL { get; set; }

        /// <summary>
        /// Is the profile private?
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// ID of profile.
        /// </summary>
        public String ProfileID { get; set; }

        /// <summary>
        /// User name of profile.
        /// </summary>
        public String UserName { get; set; }

        /// <summary>
        /// Name of profile as it appears on the page.
        /// </summary>
        public String PersonalName { get; set; }

        /// <summary>
        /// Location of profile.
        /// </summary>
        public String LocationDescription { get; set; }

        /// <summary>
        /// Count of out connections.
        /// </summary>
        public String OutConnectionTotal { get; set; }

        /// <summary>
        /// Count of in connections.
        /// </summary>
        public String InConnectionTotal { get; set; }

        /// <summary>
        /// Biography information.
        /// </summary>
        public String Biography { get; set; }

        /// <summary>
        /// Website URL information.
        /// </summary>
        public String Website { get; set; }

        /// <summary>
        /// Date profile was parsed.
        /// </summary>
        public DateTime Captured { get; set; }

        /// <summary>
        /// Were connections parsed while parsing the profile?
        /// </summary>
        public bool CapturedConnections { get; set; }

        /// <summary>
        /// Photo information.
        /// </summary>
        public List<PhotoEntry> Photos { get; private set; } = new List<PhotoEntry>();

        /// <summary>
        /// Video information.
        /// </summary>
        public List<VideoEntry> Videos { get; private set; } = new List<VideoEntry>();

        /// <summary>
        /// Connection information.
        /// </summary>
        public List<ConnectionEntry> Connections { get; private set; } = new List<ConnectionEntry>();

        /// <summary>
        /// Top 8 friend information.
        /// </summary>
        public List<Top8FriendEntry> Top8Friends { get; private set; } = new List<Top8FriendEntry>();

        /// <summary>
        /// Song information.
        /// </summary>
        public List<SongEntry> Songs { get; private set; } = new List<SongEntry>();
    }
}
