using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceTools.Data
{
    /// <summary>
    /// Describes a video entry.
    /// </summary>
    public class VideoEntry
    {
        /// <summary>
        /// Artist ID.
        /// </summary>
        public string ArtistID { get; internal set; }

        /// <summary>
        /// Artist name.
        /// </summary>
        public string ArtistName { get; internal set; }

        /// <summary>
        /// Artist URL.
        /// </summary>
        public string ArtistURL { get; internal set; }

        /// <summary>
        /// Artist user name.
        /// </summary>
        public string ArtistUserName { get; internal set; }

        /// <summary>
        /// Category.
        /// </summary>
        public string Category { get; internal set; }

        /// <summary>
        /// Custom label.
        /// </summary>
        public string CustomLabel { get; internal set; }

        /// <summary>
        /// Video description.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Video detail page URL.
        /// </summary>
        public string DetailURL { get; internal set; }

        /// <summary>
        /// Length of video in seconds.
        /// </summary>
        public string DurationInSeconds { get; internal set; }

        /// <summary>
        /// Embed type.
        /// </summary>
        public string EmbedType { get; internal set; }

        /// <summary>
        /// Embed URL.
        /// </summary>
        public string EmbedURL { get; internal set; }

        /// <summary>
        /// Key for embed API.
        /// </summary>
        public string EntityKey { get; internal set; }

        /// <summary>
        /// HLS Stream URL.
        /// </summary>
        public string HLSStreamURL { get; internal set; }

        /// <summary>
        /// Video image URL.
        /// </summary>
        public string ImageURL { get; internal set; }

        /// <summary>
        /// Are ads prohibited on this video?
        /// </summary>
        public bool IsAdsProhibited { get; internal set; }

        /// <summary>
        /// Is this video explicit?
        /// </summary>
        public bool IsExplicit { get; internal set; }

        /// <summary>
        /// Is this video full length?
        /// </summary>
        public bool IsFullLength { get; internal set; }

        /// <summary>
        /// Is this video premium?
        /// </summary>
        public bool IsPremium { get; internal set; }

        /// <summary>
        /// Is this video private?
        /// </summary>
        public bool IsPrivate { get; internal set; }

        /// <summary>
        /// Media ID.
        /// </summary>
        public string MediaID { get; internal set; }

        /// <summary>
        /// Media release ID.
        /// </summary>
        public string MediaReleaseID { get; internal set; }

        /// <summary>
        /// Media type.
        /// </summary>
        public string MediaType { get; internal set; }

        /// <summary>
        /// MP4 stream URL.
        /// </summary>
        public string MP4StreamURL { get; internal set; }

        /// <summary>
        /// Play count for this video.
        /// </summary>
        public string PlayCount { get; internal set; }

        /// <summary>
        /// Stream URL.
        /// </summary>
        public string StreamURL { get; internal set; }

        /// <summary>
        /// Video title.
        /// </summary>
        public String Title { get; set; }

        /// <summary>
        /// UID of video.
        /// </summary>
        public string UID { get; internal set; }

        public override string ToString()
        {
            return String.Format("{0}, {1}", MediaID, Title);
        }
    }
}