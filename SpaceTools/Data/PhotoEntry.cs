using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceTools.Data
{
    /// <summary>
    /// Describe photo entry.
    /// </summary>
    public class PhotoEntry
    {
        /// <summary>
        /// Caption on photo.
        /// </summary>
        public String Caption { get; set; }

        /// <summary>
        /// URL of thumbnail image of photo.
        /// </summary>
        public String ThumbnailImageURL { get; set; }

        /// <summary>
        /// URL of photo image.
        /// </summary>
        public String FullImageURL { get; set; }

        /// <summary>
        /// URL of detail page for photo.
        /// </summary>
        public String DetailPageURL { get; set; }

        /// <summary>
        /// Album name of photo, if any.
        /// </summary>
        public String AlbumName { get; set; }

        /// <summary>
        /// ID of photo.
        /// </summary>
        public String PhotoID { get; set; }

        /// <summary>
        /// Likes count on photo.
        /// </summary>
        public String LikesCount { get; set; }

        /// <summary>
        /// Connection count on photo.
        /// </summary>
        public String ConnectsCount { get; set; }

        /// <summary>
        /// Comment count on photo.
        /// </summary>
        public String CommentsCount { get; set; }

        /// <summary>
        /// Share count on photo.
        /// </summary>
        public String SharesCount { get; set; }

        /// <summary>
        /// Key for connection API for this photo.
        /// </summary>
        public String ConnectsEntityKey { get; set; }

        /// <summary>
        /// List of photo comments.
        /// </summary>
        public List<PhotoCommentEntry> Comments { get; set; } = new List<PhotoCommentEntry>();

        public override string ToString()
        {
            return String.Format("{0}, {1}", PhotoID, AlbumName);
        }
    }
}
