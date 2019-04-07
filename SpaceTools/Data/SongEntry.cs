using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceTools.Data
{
    /// <summary>
    /// Describes a song entry.
    /// </summary>
    public class SongEntry
    {
        /// <summary>
        /// ID of song.
        /// </summary>
        public String SongID { get; set; }

        /// <summary>
        /// Title of song.
        /// </summary>
        public String SongTitle { get; set; }

        /// <summary>
        /// URL of song.
        /// </summary>
        public String SongURL { get; set; }

        /// <summary>
        /// Album ID.
        /// </summary>
        public String AlbumID { get; set; }

        /// <summary>
        /// Album title.
        /// </summary>
        public String AlbumTitle { get; set; }

        /// <summary>
        /// Album URL.
        /// </summary>
        public String AlbumURL { get; set; }

        /// <summary>
        /// Artist ID.
        /// </summary>
        public String ArtistID { get; set; }

        /// <summary>
        /// Artist title.
        /// </summary>
        public String ArtistTitle { get; set; }

        /// <summary>
        /// Artist URL.
        /// </summary>
        public String ArtistURL { get; set; }
        
        /// <summary>
        /// Length of song in seconds.
        /// </summary>
        public String DurationInSeconds { get; set; }

        /// <summary>
        /// Video ID.
        /// </summary>
        public String VideoID { get; set; }

        /// <summary>
        /// Youtube video ID.
        /// </summary>
        public String YoutubeID { get; set; }

        /// <summary>
        /// Youtube video URL.
        /// </summary>
        public String YoutubeURL { get; set; }

        /// <summary>
        /// Song image URL.
        /// </summary>
        public String ImageURL { get; set; }

        /// <summary>
        /// Song image thumbnail URL.
        /// </summary>
        public String ImageThumbnailURL { get; set; }

        /// <summary>
        /// ID of genre associated with this song.
        /// </summary>
        public String GenreID { get; set; }

        /// <summary>
        /// Human readable genre name.
        /// </summary>
        public String GenreName { get; set; }

        /// <summary>
        /// Media ID.
        /// </summary>
        public String MediaID { get; set; }

        /// <summary>
        /// Media type.
        /// </summary>
        public String MediaType { get; set; }

        /// <summary>
        /// UID of song.
        /// </summary>
        public String UID { get; set; }

        /// <summary>
        /// Play count of song.
        /// </summary>
        public String PlayCount { get; set; }

        /// <summary>
        /// Label of song.
        /// </summary>
        public String Label { get; set; }

        /// <summary>
        /// Release date.
        /// </summary>
        public String ReleaseDate { get; set; }

        /// <summary>
        /// Is the song premium?
        /// </summary>
        public bool IsPremium { get; set; }

        /// <summary>
        /// Is the song explicit?
        /// </summary>
        public bool IsExplicit { get; set; }

        /// <summary>
        /// Is this full length recording?
        /// </summary>
        public bool IsFullLength { get; set; }

        /// <summary>
        /// Are ads prohibited on this song?
        /// </summary>
        public bool IsAdsProhibited { get; set; }

        public override string ToString()
        {
            return String.Format("{0}, {1}", SongID, SongTitle);
        }
    }
}
