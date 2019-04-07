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

namespace SpaceTools.Tools.Crawler
{
    /// <summary>
    /// Crawl profiles and create JSON files. Uses connections for branching.
    /// </summary>
    public class ProfileCrawler : ILog, IDisposable
    {
        /// <summary>
        /// User name to use for root of crawl.
        /// </summary>
        private string SeedUserName { get; set; }

        /// <summary>
        /// Maximum depth to crawl.
        /// </summary>
        /// <remarks>Set 1 higher than branching factor, as examining connections counts as towards depth.</remarks>
        private int MaxDepth { get; set; }

        /// <summary>
        /// Delay between page requests.
        /// </summary>
        private int DelayBetweenPages { get; set; }

        /// <summary>
        /// Delay between API requests.
        /// </summary>
        private int DelayBetweenAPIRequests { get; set; }

        /// <summary>
        /// Hashkey to use for API calls.
        /// </summary>
        private String HashKey { get; set; }

        /// <summary>
        /// If not null, to be considered for crawling, the profile location must contain this string.
        /// </summary>
        private String LocationCriteria { get; set; }

        /// <summary>
        /// If false, profiles without location strings will be ignored.
        /// </summary>
        private bool AllowEmptyLocations { get; set; }

        /// <summary>
        /// Directory to store profiles in.
        /// </summary>
        private String StoreDirectory { get; set; }

        /// <summary>
        /// Log file writer.
        /// </summary>
        private StreamWriter LogFile { get; set; }

        /// <summary>
        /// Should photos be captured?
        /// </summary>
        private bool CapturePhotos { get; set; }

        /// <summary>
        /// Crawl profiles and create JSON files. Uses connections for branching.
        /// </summary>
        /// <param name="storeDirectory">Directory to store profiles in.</param>
        /// <param name="hashKey">Hashkey to use for API calls.</param>
        /// <param name="seedUserName">User name to use for root of crawl.</param>
        /// <param name="delayBetweenPages">Delay between page requests.</param>
        /// <param name="delayBetweenAPIRequests">Delay between API requests.</param>
        /// <param name="maxDepth">Maximum depth to crawl.</param>
        /// <param name="locationCriteria">If not null, to be considered for crawling, the profile location must contain this string.</param>
        /// <param name="allowEmptyLocations">If false, profiles without location strings will be ignored.</param>
        /// <param name="capturePhotos">Should photos be captured?</param>
        public ProfileCrawler(
            String storeDirectory,
            String hashKey, 
            String seedUserName, 
            int delayBetweenPages, 
            int delayBetweenAPIRequests, 
            int maxDepth,
            String locationCriteria,
            bool allowEmptyLocations,
            bool capturePhotos)
        {
            StoreDirectory = storeDirectory;
            HashKey = hashKey;
            SeedUserName = seedUserName;
            DelayBetweenPages = delayBetweenPages;
            DelayBetweenAPIRequests = delayBetweenAPIRequests;
            MaxDepth = maxDepth;
            LocationCriteria = locationCriteria;
            AllowEmptyLocations = allowEmptyLocations;
            CapturePhotos = capturePhotos;

            try
            {
                LogFile = File.AppendText(Path.Combine(storeDirectory, String.Format("crawl_{0}.log", DateTime.Now.ToString("yyyy_MM_dd"))));
            }
            catch(Exception e)
            {
                LogFile = null;
            }
        }

        /// <summary>
        /// Log a message to the log file.
        /// </summary>
        /// <param name="message"></param>
        public void Log(String message)
        {
            String formattedMessage = String.Format("{0}: {1}", DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss"), message);
            if (LogFile != null)
            {
                LogFile.WriteLine(formattedMessage);
                LogFile.Flush();
            }
            System.Console.WriteLine(formattedMessage);
        }

        /// <summary>
        /// Start crawling from seed user name.
        /// </summary>
        public void Crawl()
        {
            Log(String.Format("Started Crawl: Seed={0}, MaxDepth={1}, LocationCriteria={2}, AllowEmptyLocations={3}, Directory={4}", 
                SeedUserName, MaxDepth, LocationCriteria, AllowEmptyLocations.ToString(), StoreDirectory));
            Log(String.Format("Using HashKey={0}", HashKey));

            #region Clear locks
            try
            {
                Log(String.Format("Clearing Locks...", HashKey));
                var directories = Directory.GetDirectories(StoreDirectory);
                foreach (String directory in directories)
                {
                    try
                    {
                        String lockFileName = Path.Combine(directory, UserNameCrawlLock(new DirectoryInfo(directory).Name));
                        if (File.Exists(lockFileName))
                        {
                            File.Delete(lockFileName);
                        }
                    }
                    catch(Exception l)
                    {
                        Log(String.Format("Error Clearing Lock: Directory={0}, {1}", directory, l?.Message));
                    }
                }
                Log(String.Format("Locks Cleared", HashKey));
            }
            catch(Exception d)
            {
                Log(String.Format("Error Getting Lock Directories: {0}", d?.Message));
            }
            #endregion

            CrawlNode(SeedUserName, DelayBetweenPages, 0);
        }

        /// <summary>
        /// Crawl an individual profile, then its connections.
        /// </summary>
        /// <param name="userName">User name of profile to parse</param>
        /// <param name="delayBetweenPages">Delay between pages</param>
        /// <param name="depth">Depth to parse from seed</param>
        public void CrawlNode(String userName, int delayBetweenPages, int depth)
        {
            if(depth + 1 > MaxDepth)
            {
                return;
            }

            if (File.Exists(UserNameCrawlLock(userName)))
            {
                Log(String.Format(@"Skip Locked Profile: UserName={0}", userName));
                return;
            }

            #region Check if all connections have been parsed
            if(File.Exists(UserNameCompletePath(userName)))
            {
                try
                {
                    CompleteFile checkFile = JsonConvert.DeserializeObject<CompleteFile>(File.ReadAllText(UserNameCompletePath(userName)));
                    if(checkFile != null && checkFile.AllConnectionsParsed)
                    {
                        Log(String.Format(@"Skip Completed Profile: UserName={0}", userName));
                        return;
                    }
                }
                catch(Exception chk)
                {
                    Log(String.Format(@"Error Checking Lock File: UserName={0}, {1}", userName, chk?.Message));
                }
            }
            #endregion

            #region Create Directory if not exists
            try
            {
                String path = Path.Combine(StoreDirectory,userName);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception e)
            {
                Log(String.Format("Error Creating Directory: UserName={0}", userName));
            }
            #endregion

            #region Create Lock file
            try
            {
                File.Create(UserNameCrawlLock(userName)).Dispose();
            }
            catch (Exception e)
            {
                Log(String.Format(@"Error Creating Lock File: UserName={0}, {1}", userName, e?.Message));
            }
            #endregion

            Profile profile = null;

            if (!UserNameHasBeenParsed(userName))
            {
                bool captureConnections = CanCaptureConnections(userName);

                Log(String.Format(@"Parse Profile: UserName={0}, Depth={1}/{2}, CaptureConnections={3}",
                    userName, depth.ToString(), MaxDepth.ToString(), captureConnections.ToString()));

                profile = new ProfileParser().Parse(
                    this,
                    HashKey,
                    userName,
                    DelayBetweenPages,
                    DelayBetweenAPIRequests,
                    captureConnections);

                if (profile != null)
                {
                    ProfileFinished(profile);

                    #region Download additional files if passes check
                    if (CapturePhotos)
                    {
                        #region Profile Photos
                        Log(String.Format(@"Download Profile Photos: UserName={0}", profile.UserName));
                        if (!String.IsNullOrEmpty(profile.ProfileImageURL)
                        && !File.Exists(Path.Combine(StoreDirectory, profile.UserName, "profile.jpg"))
                        && !File.Exists(Path.Combine(StoreDirectory, profile.UserName, "profile.jpg.error")))
                        {
                            bool success = CrawlUtil.GetFile(profile.ProfileImageURL, Path.Combine(StoreDirectory, profile.UserName, "profile.jpg"), 30000);
                            if (!success)
                            {
                                File.Create(Path.Combine(StoreDirectory, profile.UserName, "profile.jpg.error")).Dispose();
                                Log(String.Format(@"Error Profile Photo Thumbnail: UserName={0}", profile.UserName));
                            }
                        }

                        if (!String.IsNullOrEmpty(profile.ProfileThumbnailImageURL)
                        && !File.Exists(Path.Combine(StoreDirectory, profile.UserName, "profile_sm.jpg"))
                        && !File.Exists(Path.Combine(StoreDirectory, profile.UserName, "profile_sm.jpg.error")))
                        {
                            bool success = CrawlUtil.GetFile(profile.ProfileThumbnailImageURL, Path.Combine(StoreDirectory, profile.UserName, "profile_sm.jpg"), 30000);
                            if (!success)
                            {
                                File.Create(Path.Combine(StoreDirectory, profile.UserName, "profile_sm.jpg.error")).Dispose();
                                Log(String.Format(@"Error Profile Photo Thumbnail: UserName={0}", profile.UserName));
                            }
                        }
                        #endregion

                        #region Photos Albums
                        if (profile.Photos != null && profile.Photos.Count > 0)
                        {
                            Log(String.Format(@"Download Photos: UserName={0}", profile.UserName));

                            #region Ensure Photos directory exists
                            String photoAlbumsPath = Path.Combine(UserNameDirectoryPath(profile.UserName), "Photos");
                            if (!Directory.Exists(photoAlbumsPath))
                            {
                                Directory.CreateDirectory(photoAlbumsPath);
                            }
                            #endregion

                            foreach (PhotoEntry entry in profile.Photos)
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
                                                Log(String.Format(@"Download Photo Thumbnail: UserName={0}, PhotoID={1}, Album={2}", profile.UserName, entry.PhotoID, entry.AlbumName));
                                                bool success = CrawlUtil.GetFile(entry.ThumbnailImageURL,
                                                    Path.Combine(picturePath, String.Format("{0}_sm.jpg", entry.PhotoID)),
                                                    30000);

                                                if (!success)
                                                {
                                                    File.Create(Path.Combine(picturePath, String.Format("{0}_sm.error", entry.PhotoID))).Dispose();
                                                    Log(String.Format(@"Error Downloading Photo Thumbnail: UserName={0}, PhotoID={1}, Album={2}", profile.UserName, entry.PhotoID, entry.AlbumName));
                                                }
                                            }
                                            #endregion

                                            #region Download Full Photo
                                            if (!String.IsNullOrEmpty(entry.FullImageURL)
                                            && !File.Exists(Path.Combine(picturePath, String.Format("{0}.jpg", entry.PhotoID)))
                                            && !File.Exists(Path.Combine(picturePath, String.Format("{0}.error", entry.PhotoID))))
                                            {
                                                Log(String.Format(@"Download Photo: UserName={0}, PhotoID={1}, Album={2}", profile.UserName, entry.PhotoID, entry.AlbumName));
                                                bool success = CrawlUtil.GetFile(entry.FullImageURL,
                                                    Path.Combine(picturePath, String.Format("{0}.jpg", entry.PhotoID)),
                                                    30000);

                                                if (!success)
                                                {
                                                    File.Create(Path.Combine(picturePath, String.Format("{0}.error", entry.PhotoID))).Dispose();
                                                    Log(String.Format(@"Error Downloading Photo: UserName={0}, PhotoID={1}, Album={2}", profile.UserName, entry.PhotoID, entry.AlbumName));
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
                                    Log(String.Format(@"Error Downloading Photo: UserName={0}, PhotoID={1}", profile.UserName, entry?.PhotoID));
                                }
                            }
                        }
                        #endregion

                        #region Song Artwork
                        if (profile.Songs != null && profile.Songs.Count > 0)
                        {
                            Log(String.Format(@"Download Song Artwork: UserName={0}", profile.UserName));

                            #region Ensure Photos directory exists
                            String songArtworkPath = Path.Combine(UserNameDirectoryPath(profile.UserName), "Song_Artwork");
                            if (!Directory.Exists(songArtworkPath))
                            {
                                Directory.CreateDirectory(songArtworkPath);
                            }
                            #endregion

                            foreach (SongEntry entry in profile.Songs)
                            {
                                #region Download Thumbnail
                                String thumbnailFileName = entry.ImageThumbnailURL?.Replace(@"/", "___")?.Replace(":", "---");

                                if (!String.IsNullOrEmpty(thumbnailFileName)
                                && !File.Exists(Path.Combine(songArtworkPath, thumbnailFileName))
                                && !File.Exists(Path.Combine(songArtworkPath, String.Format("{0}.error", thumbnailFileName))))
                                {
                                    Log(String.Format(@"Download Song Artwork Thumbnail: UserName={0}, Name={1}", profile.UserName, thumbnailFileName));
                                    bool success = CrawlUtil.GetFile(entry.ImageThumbnailURL,
                                        Path.Combine(songArtworkPath, thumbnailFileName),
                                        30000);

                                    if (!success)
                                    {
                                        File.Create(Path.Combine(songArtworkPath, String.Format("{0}.error", thumbnailFileName))).Dispose();
                                        Log(String.Format(@"Error Downloading Song Artwork Thumbnail: UserName={0}, Name={1}", profile.UserName, thumbnailFileName));
                                    }
                                }
                                #endregion

                                #region Download Full Image
                                String imageFileName = entry.ImageURL?.Replace(@"/", "___")?.Replace(":", "---");

                                if (!String.IsNullOrEmpty(imageFileName)
                                && !File.Exists(Path.Combine(songArtworkPath, imageFileName))
                                && !File.Exists(Path.Combine(songArtworkPath, String.Format("{0}.error", imageFileName))))
                                {
                                    Log(String.Format(@"Download Song Artwork: UserName={0}, Name={1}", profile.UserName, imageFileName));
                                    bool success = CrawlUtil.GetFile(entry.ImageURL,
                                        Path.Combine(songArtworkPath, imageFileName),
                                        60000);

                                    if (!success)
                                    {
                                        File.Create(Path.Combine(songArtworkPath, String.Format("{0}.error", imageFileName))).Dispose();
                                        Log(String.Format(@"Error Downloading Song Artwork: UserName={0}, Name={1}", profile.UserName, imageFileName));
                                    }
                                }
                                #endregion

                                //Wait between each photo.
                                Thread.Sleep(CrawlUtil.GetVariableDelay(200));
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
            }
            else
            {
                Log(String.Format(@"Load Profile: UserName={0}", userName));

                try
                {
                    profile = JsonConvert.DeserializeObject<Profile>(File.ReadAllText(UserNameProfilePath(userName)));
                }
                catch (Exception e)
                {
                    Log(String.Format(@"Error Loading Profile: UserName={0}, {1}", userName, e?.Message));
                }
            }

            if(profile != null)
            {
                CompleteFile completeFile = null;

                #region Ensure complete file has been created and populated
                if (!File.Exists(UserNameCompletePath(userName)))
                {
                    try
                    {
                        completeFile = new CompleteFile(userName);
                        if (profile.Connections != null)
                        {
                            foreach (ConnectionEntry c in profile.Connections)
                            {
                                completeFile.AllConnectionsParsed = false;
                                completeFile.ConnectionsParsed.Add(
                                    new ConnectionParsedEntry()
                                    {
                                        UserName = c.UserName,
                                        Parsed = false,
                                        DateParsed = null
                                    }
                                    );
                            }
                        }

                        File.WriteAllText(
                           UserNameCompletePath(userName),
                           JsonConvert.SerializeObject(completeFile, Formatting.Indented));
                    }
                    catch(Exception e)
                    {
                        Log(String.Format(@"Error Creating Complete File: UserName={0}", e?.Message));
                    }
                }
                else
                {
                    completeFile = JsonConvert.DeserializeObject<CompleteFile>(File.ReadAllText(UserNameCompletePath(userName)));
                }
                #endregion
                
                foreach (ConnectionEntry c in profile.Connections)
                {
                    ConnectionParsedEntry parsedEntry = completeFile.ConnectionsParsed.Where(x => String.Equals(x.UserName, c.UserName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (parsedEntry != null)
                    {
                        if (!parsedEntry.Parsed)
                        {
                            CrawlNode(c.UserName, DelayBetweenPages, depth + 1);

                            #region Update complete file
                            try
                            {
                                parsedEntry.Parsed = true;
                                parsedEntry.DateParsed = DateTime.Now;

                                File.WriteAllText(
                               UserNameCompletePath(userName),
                               JsonConvert.SerializeObject(completeFile, Formatting.Indented));
                            }
                            catch (Exception p)
                            {
                                Log(String.Format(@"Error Updating Complete Entry: UserName={0}, ConnectionUserName={1}, {2]", userName, c.UserName, p?.Message));
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        Log(String.Format(@"Error Missing Connection Complete Entry: UserName={0}, ConnectionUserName={1}", userName, c.UserName));
                    }
                }

                #region Update complete flag
                try
                {
                    Log(String.Format(@"All Connections Parsed: UserName={0}", userName));
                    completeFile.AllConnectionsParsed = true;

                    File.WriteAllText(
                   UserNameCompletePath(userName),
                   JsonConvert.SerializeObject(completeFile, Formatting.Indented));
                }
                catch (Exception p)
                {
                    Log(String.Format(@"Error Updating Complete File: UserName={0}, {1}", userName, p?.Message));
                }
                #endregion
            }
        }

        /// <summary>
        /// Occurs when a profile has been fully parsed.
        /// </summary>
        /// <param name="profile">Profile that has been filled</param>
        public void ProfileFinished(Profile profile)
        {
            try
            {
                if (profile?.UserName != null)
                {
                    String path = UserNameDirectoryPath(profile.UserName);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    String profileFilePath = "";
                    try
                    {
                        profileFilePath = UserNameProfilePath(profile.UserName);
                        File.WriteAllText(
                            profileFilePath,
                            JsonConvert.SerializeObject(profile, Formatting.Indented));
                    }
                    catch (Exception e)
                    {
                        Log(String.Format("Error Creating Profile File: {0}", profileFilePath));
                    }
                }
            }
            catch(Exception e)
            {
                Log(String.Format("Error Creating Profile Directory: {0}", e?.Message));
            }
        }

        /// <summary>
        /// Has the given user name already been parsed?
        /// </summary>
        /// <param name="userName">User Name to parse</param>
        /// <returns>true if can be parsed</returns>
        public bool UserNameHasBeenParsed(String userName)
        {
            return (File.Exists(UserNameProfilePath(userName)));
        }

        /// <summary>
        /// Should connections be captured for the given username?
        /// Not capturing connections will prevent other profiles from being parsed from this one.
        /// </summary>
        /// <param name="userName">User Name to capture connections</param>
        /// <returns>true if connections should be captured</returns>
        public bool CanCaptureConnections(String userName)
        {
            if (!string.IsNullOrEmpty(LocationCriteria))
            {
                String location = CrawlUtil.GetUserLocation(userName);

                if (AllowEmptyLocations)
                {
                    return String.IsNullOrEmpty(location) || (location.ToLower().Contains(LocationCriteria));
                }
                else
                {
                    return !String.IsNullOrEmpty(location) && (location.ToLower().Contains(LocationCriteria));
                }
            }
            else
            {
                return true;
            }
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

        /// <summary>
        /// Returns the path to the file containing complete flags.
        /// </summary>
        /// <param name="userName">User name as it appears in the profile URL</param>
        /// <returns></returns>
        public String UserNameCompletePath(String userName)
        {
            return Path.Combine(StoreDirectory, userName, String.Format(@"{0}.complete.json", userName));
        }

        /// <summary>
        /// Returns the path to the file used as a crawl lock while crawling.
        /// </summary>
        /// <param name="userName">User name as it appears in the profile URL</param>
        /// <returns></returns>
        public String UserNameCrawlLock(String userName)
        {
            return Path.Combine(StoreDirectory, userName, String.Format(@"{0}.crawl.lock", userName));
        }

        public void Dispose()
        {
            if(LogFile != null)
            {
                LogFile.Flush();
                LogFile.Close();
            }
        }
    }
}
