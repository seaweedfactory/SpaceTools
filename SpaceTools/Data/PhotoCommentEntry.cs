using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceTools.Data
{
    /// <summary>
    /// Describe a comment on a photo.
    /// </summary>
    public class PhotoCommentEntry
    {
        /// <summary>
        /// Profile URL.
        /// </summary>
        public String ProfileURL { get; set; }

        /// <summary>
        /// Profile containing photo.
        /// </summary>
        public String UserName { get; set; }

        /// <summary>
        /// Comment on photo as a string.
        /// </summary>
        public String Comment { get; set; }

        /// <summary>
        /// Comment on photo which may contain HTML.
        /// </summary>
        public String CommentHTML { get; set; }

        /// <summary>
        /// Date of comment as UTC.
        /// </summary>
        public String DateTimeUTC { get; set; }

        /// <summary>
        /// Humna readable date of comment.
        /// </summary>
        public String DateTimeDisplay { get; set; }

        /// <summary>
        /// URL of profile thumbnail image of commenter.
        /// </summary>
        public String ThumbnailImageURL { get; set; }
        
        public override string ToString()
        {
            return String.Format("{0}, {1}, {2}", UserName, DateTimeDisplay, Comment);
        }
    }
}
