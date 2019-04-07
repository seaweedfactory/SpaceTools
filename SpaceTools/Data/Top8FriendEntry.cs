using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceTools.Data
{
    /// <summary>
    /// Describes a Top 8 friends entry.
    /// </summary>
    public class Top8FriendEntry
    {
        /// <summary>
        /// Friend URL.
        /// </summary>
        public String UserURL { get; set; }

        /// <summary>
        /// Friend name as it appears on profile.
        /// </summary>
        public String UserName { get; set; }

        /// <summary>
        /// URL of profile thumbnail image.
        /// </summary>
        public String ThumbnailURL { get; set; }

        /// <summary>
        /// ID of profile.
        /// </summary>
        public String ProfileID { get; set; }

        public override string ToString()
        {
            return String.Format("{0}, {1}", ProfileID, UserName);
        }
    }
}
