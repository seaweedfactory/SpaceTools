using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceTools.Tools.LocationCrawl
{
    /// <summary>
    /// Information about a profile location.
    /// </summary>
    public class ProfileLocationRecord
    {
        /// <summary>
        /// Profile name.
        /// </summary>
        public String UserName { get; set; }

        /// <summary>
        /// Name as it appears on the profile.
        /// </summary>
        public String PersonalName { get; set; }

        /// <summary>
        /// Location from profile.
        /// </summary>
        public String Location { get; set; }

        /// <summary>
        /// Locations that lead to profile using crawler.
        /// </summary>
        public String ConnectedFromLocation { get; set; }

        /// <summary>
        /// Error that occurred during parsing.
        /// </summary>
        public String Error { get; set; }

        public override string ToString()
        {
            return String.Format("{0}, {1}, {2}", UserName, PersonalName, Location);
        }
    }
}
