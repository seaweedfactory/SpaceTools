using Newtonsoft.Json;
using SpaceTools.Data;
using SpaceTools.Tools.Parser;
using SpaceTools.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpaceTools.Tools.Downloader
{
    /// <summary>
    /// Download a profile as a JSON file. Optionally download images from the profile.
    /// </summary>
    public class ProfileDownloader : IDisposable
    {
        /// <summary>
        /// Profile to download.
        /// </summary>
        private String UserName { get; set; }

        /// <summary>
        /// Directory to store profiles in.
        /// </summary>
        private String StoreDirectory { get; set; }

        /// <summary>
        /// Hashkey for API calls.
        /// </summary>
        private String HashKey { get; set; }
        
        /// <summary>
        /// Optional function to check if photos should be downloaded.
        /// </summary>
        private Func<Profile, bool> DownloadPhotosCheck { get; set; }

        /// <summary>
        /// Should photos be captured?
        /// </summary>
        private bool CapturePhotos { get; set; }

        /// <summary>
        /// Should connections be parsed?
        /// </summary>
        private bool CaptureConnections { get; set; }

        /// <summary>
        /// Profile object to fill.
        /// </summary>
        private Profile Profile { get; set; }

        /// <summary>
        /// Logging functions.
        /// </summary>
        private Logger Logger { get; set; }

        /// <summary>
        /// Download a profile as a JSON file. Optionally download images from the profile.
        /// </summary>
        /// <param name="userName">Profile to download.</param>
        /// <param name="storeDirectory">Directory to store profiles in.</param>
        /// <param name="hashKey">Hashkey for API calls.</param>
        /// <param name="capturePhotos">Should photos be captured?</param>
        /// <param name="captureConnections">Should connections be parsed?</param>
        /// <param name="downloadPhotosCheck">Optional function to check if photos should be downloaded.</param>
        public ProfileDownloader(
            String userName, 
            String storeDirectory, 
            String hashKey, 
            bool capturePhotos, 
            bool captureConnections,
            Func<Profile, bool> downloadPhotosCheck = null)
        {
            UserName = userName;
            StoreDirectory = storeDirectory;
            HashKey = hashKey;
            CapturePhotos = capturePhotos;
            CaptureConnections = captureConnections;
            DownloadPhotosCheck = downloadPhotosCheck;
        }

        /// <summary>
        /// Startdownload process.
        /// </summary>
        public void Download()
        {
            #region Ensure path exists
            String path = UserNameDirectoryPath(UserName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            #endregion

            Logger = new Logger(UserNameDirectoryPath(UserName), String.Format("download_{0}", UserName));

            Logger.Log(String.Format(@"Start Download Process: UserName={0}", UserName));

            if (!File.Exists(UserNameProfilePath(UserName)))
            {
                #region Download Profile
                Logger.Log(String.Format(@"Download Profile: UserName={0}", UserName));
                ProfileParser profileParse = new ProfileParser();
                Profile = profileParse.Parse(Logger, HashKey, UserName, 250, 200, CaptureConnections);
                if (Profile == null)
                {
                    Logger.Log(String.Format(@"Empty Profile: UserName={0}", UserName));
                    return;
                }
                Logger.Log(String.Format(@"Downloaded Profile: UserName={0}", UserName));

                try
                {
                    File.WriteAllText(
                                UserNameProfilePath(UserName),
                                JsonConvert.SerializeObject(Profile, Formatting.Indented));
                }
                catch (Exception e)
                {
                    Logger.Log(String.Format(@"Error Saving Profile: UserName={0}, {1}", UserName, e?.Message));
                    return;
                }

                Logger.Log(String.Format(@"Save Profile JSON: UserName={0}", UserName));
                #endregion
            }
            else
            {
                #region Load Profile JSON
                Logger.Log(String.Format(@"Load Profile JSON: UserName={0}", UserName));
                try
                {
                    Profile = JsonConvert.DeserializeObject<Profile>(File.ReadAllText(UserNameProfilePath(UserName)));
                    if (Profile == null)
                    {
                        Logger.Log(String.Format(@"Empty Profile: UserName={0}", UserName));
                        return;
                    }
                }
                catch (Exception e)
                {
                    Logger.Log(String.Format(@"Error loading profile: UserName={0}, {1}", UserName, e?.Message));
                    return;
                }
                Logger.Log(String.Format(@"Loaded Profile JSON: UserName={0}", UserName));
                #endregion
            }

            //Download additional files if passes check
            if (CapturePhotos)
            {
                if (DownloadPhotosCheck == null || (Profile != null && DownloadPhotosCheck.Invoke(Profile)))
                {
                    #region Profile Photos
                    Logger.Log(String.Format(@"Download Profile Photos: UserName={0}", UserName));
                    if (!String.IsNullOrEmpty(Profile.ProfileImageURL)
                    && !File.Exists(Path.Combine(StoreDirectory, UserName, "profile.jpg"))
                    && !File.Exists(Path.Combine(StoreDirectory, UserName, "profile.jpg.error")))
                    {
                        bool success = CrawlUtil.GetFile(Profile.ProfileImageURL, Path.Combine(StoreDirectory, UserName, "profile.jpg"), 30000);
                        if (!success)
                        {
                            File.Create(Path.Combine(StoreDirectory, UserName, "profile.jpg.error")).Dispose();
                            Logger.Log(String.Format(@"Error Profile Photo Thumbnail: UserName={0}", UserName));
                        }
                    }

                    if (!String.IsNullOrEmpty(Profile.ProfileThumbnailImageURL)
                    && !File.Exists(Path.Combine(StoreDirectory, UserName, "profile_sm.jpg"))
                    && !File.Exists(Path.Combine(StoreDirectory, UserName, "profile_sm.jpg.error")))
                    {
                        bool success = CrawlUtil.GetFile(Profile.ProfileThumbnailImageURL, Path.Combine(StoreDirectory, UserName, "profile_sm.jpg"), 30000);
                        if (!success)
                        {
                            File.Create(Path.Combine(StoreDirectory, UserName, "profile_sm.jpg.error")).Dispose();
                            Logger.Log(String.Format(@"Error Profile Photo Thumbnail: UserName={0}", UserName));
                        }
                    }
                    #endregion

                    #region Photos Albums
                    if (Profile.Photos != null && Profile.Photos.Count > 0)
                    {
                        Logger.Log(String.Format(@"Download Photos: UserName={0}", UserName));

                        #region Ensure Photos directory exists
                        String photoAlbumsPath = Path.Combine(UserNameDirectoryPath(UserName), "Photos");
                        if (!Directory.Exists(photoAlbumsPath))
                        {
                            Directory.CreateDirectory(photoAlbumsPath);
                        }
                        #endregion

                        foreach (PhotoEntry entry in Profile.Photos)
                        {
                            try
                            {
                                if (!String.IsNullOrEmpty(entry.PhotoID))
                                {
                                    String picturePath = photoAlbumsPath;
                                    if (!String.IsNullOrEmpty(entry.AlbumName))
                                    {
                                        #region Ensure Photo album directory exists
                                        picturePath = Path.Combine(picturePath, entry.AlbumName);
                                        if (!Directory.Exists(picturePath))
                                        {
                                            Directory.CreateDirectory(picturePath);
                                        }
                                        #endregion

                                        #region Download Thumbnail
                                        if (!String.IsNullOrEmpty(entry.ThumbnailImageURL)
                                        && !File.Exists(Path.Combine(picturePath, String.Format("{0}_sm.jpg", entry.PhotoID)))
                                        && !File.Exists(Path.Combine(picturePath, String.Format("{0}_sm.error", entry.PhotoID))))
                                        {
                                            Logger.Log(String.Format(@"Download Photo Thumbnail: UserName={0}, PhotoID={1}, Album={2}", UserName, entry.PhotoID, entry.AlbumName));
                                            bool success = CrawlUtil.GetFile(entry.ThumbnailImageURL,
                                                Path.Combine(picturePath, String.Format("{0}_sm.jpg", entry.PhotoID)),
                                                30000);

                                            if (!success)
                                            {
                                                File.Create(Path.Combine(picturePath, String.Format("{0}_sm.error", entry.PhotoID))).Dispose();
                                                Logger.Log(String.Format(@"Error Downloading Photo Thumbnail: UserName={0}, PhotoID={1}, Album={2}", UserName, entry.PhotoID, entry.AlbumName));
                                            }
                                        }
                                        #endregion

                                        #region Download Full Photo
                                        if (!String.IsNullOrEmpty(entry.FullImageURL)
                                        && !File.Exists(Path.Combine(picturePath, String.Format("{0}.jpg", entry.PhotoID)))
                                        && !File.Exists(Path.Combine(picturePath, String.Format("{0}.error", entry.PhotoID))))
                                        {
                                            Logger.Log(String.Format(@"Download Photo: UserName={0}, PhotoID={1}, Album={2}", UserName, entry.PhotoID, entry.AlbumName));
                                            bool success = CrawlUtil.GetFile(entry.FullImageURL,
                                                Path.Combine(picturePath, String.Format("{0}.jpg", entry.PhotoID)),
                                                30000);

                                            if (!success)
                                            {
                                                File.Create(Path.Combine(picturePath, String.Format("{0}.error", entry.PhotoID))).Dispose();
                                                Logger.Log(String.Format(@"Error Downloading Photo: UserName={0}, PhotoID={1}, Album={2}", UserName, entry.PhotoID, entry.AlbumName));
                                            }
                                        }
                                        #endregion
                                    }

                                    //Wait between each photo.
                                    Thread.Sleep(CrawlUtil.GetVariableDelay(200));
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Log(String.Format(@"Error Downloading Photo: UserName={0}, PhotoID={1}", UserName, entry?.PhotoID));
                            }
                        }
                    }
                    #endregion

                    #region Song Artwork
                    if (Profile.Songs != null && Profile.Songs.Count > 0)
                    {
                        Logger.Log(String.Format(@"Download Song Artwork: UserName={0}", UserName));

                        #region Ensure Photos directory exists
                        String songArtworkPath = Path.Combine(UserNameDirectoryPath(UserName), "Song_Artwork");
                        if (!Directory.Exists(songArtworkPath))
                        {
                            Directory.CreateDirectory(songArtworkPath);
                        }
                        #endregion

                        foreach (SongEntry entry in Profile.Songs)
                        {
                            #region Download Thumbnail
                            String thumbnailFileName = entry.ImageThumbnailURL?.Replace(@"/", "___")?.Replace(":", "---");

                            if (!String.IsNullOrEmpty(thumbnailFileName)
                            && !File.Exists(Path.Combine(songArtworkPath, thumbnailFileName))
                            && !File.Exists(Path.Combine(songArtworkPath, String.Format("{0}.error", thumbnailFileName))))
                            {
                                Logger.Log(String.Format(@"Download Song Artwork Thumbnail: UserName={0}, Name={1}", UserName, thumbnailFileName));
                                bool success = CrawlUtil.GetFile(entry.ImageThumbnailURL,
                                    Path.Combine(songArtworkPath, thumbnailFileName),
                                    30000);

                                if (!success)
                                {
                                    File.Create(Path.Combine(songArtworkPath, String.Format("{0}.error", thumbnailFileName))).Dispose();
                                    Logger.Log(String.Format(@"Error Downloading Song Artwork Thumbnail: UserName={0}, Name={1}", UserName, thumbnailFileName));
                                }
                            }
                            #endregion

                            #region Download Full Image
                            String imageFileName = entry.ImageURL?.Replace(@"/", "___")?.Replace(":", "---");

                            if (!String.IsNullOrEmpty(imageFileName)
                            && !File.Exists(Path.Combine(songArtworkPath, imageFileName))
                            && !File.Exists(Path.Combine(songArtworkPath, String.Format("{0}.error", imageFileName))))
                            {
                                Logger.Log(String.Format(@"Download Song Artwork: UserName={0}, Name={1}", UserName, imageFileName));
                                bool success = CrawlUtil.GetFile(entry.ImageURL,
                                    Path.Combine(songArtworkPath, imageFileName),
                                    60000);

                                if (!success)
                                {
                                    File.Create(Path.Combine(songArtworkPath, String.Format("{0}.error", imageFileName))).Dispose();
                                    Logger.Log(String.Format(@"Error Downloading Song Artwork: UserName={0}, Name={1}", UserName, imageFileName));
                                }
                            }
                            #endregion

                            //Wait between each photo.
                            Thread.Sleep(CrawlUtil.GetVariableDelay(200));
                        }
                    }
                    #endregion
                }
            }

            Logger.Log(String.Format(@"Done: UserName={0}", UserName));
        }

        /// <summary>
        /// Returns the directory to store information related to a user name in.
        /// </summary>
        /// <param name="userName">User name as it appears in the profile URL</param>
        /// <returns></returns>
        public String UserNameDirectoryPath(String userName)
        {
            return Path.Combine(StoreDirectory, userName);
        }

        /// <summary>
        /// Returns the path to the stored profile JSON file.
        /// </summary>
        /// <param name="userName">User name as it appears in the profile URL</param>
        /// <returns></returns>
        public String UserNameProfilePath(String userName)
        {
            return Path.Combine(StoreDirectory, userName, String.Format(@"{0}.profile.json", userName));
        }

        public void Dispose()
        {
            Logger?.Dispose();
        }
    }
}
