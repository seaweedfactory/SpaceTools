using SpaceTools.Data;
using SpaceTools.Tools.Downloader;
using SpaceTools.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceTools.Tools.ListDownloader
{
    /// <summary>
    /// Download a list of profiles.
    /// </summary>
    public class ListDownloader
    {
        /// <summary>
        /// Hashkey for API calls.
        /// </summary>
        public String HashKey { get; private set; }

        /// <summary>
        /// Directory to store profiles in.
        /// </summary>
        public String StoreDirectory { get; private set; }

        /// <summary>
        /// Path to list file containing profile list.
        /// </summary>
        public String ListFileName { get; private set; }

        /// <summary>
        /// Should photos be captured?
        /// </summary>
        public bool CapturePhotos { get; private set; }

        /// <summary>
        /// Should connections be parsed?
        /// </summary>
        public bool CaptureConnections { get; private set; }

        /// <summary>
        /// Optional function to check if photos should be downloaded.
        /// </summary>
        public Func<Profile, bool> DownloadPhotoCheck { get; private set; }

        /// <summary>
        /// Download a list of profiles.
        /// </summary>
        /// <param name="listFileName">Path to list file containing profile list.</param>
        /// <param name="storeDirectory">Directory to store profiles in.</param>
        /// <param name="hashKey">Hashkey for API calls.</param>
        /// <param name="capturePhotos">Should photos be captured?</param>
        /// <param name="captureConnections">Should connections be parsed?</param>
        /// <param name="downloadPhotoCheck">Optional function to check if photos should be downloaded.</param>
        public ListDownloader(
            String listFileName,
            String storeDirectory,
            String hashKey,
            bool capturePhotos,
            bool captureConnections,
            Func<Profile, bool> downloadPhotoCheck = null)
        {
            ListFileName = listFileName;
            StoreDirectory = storeDirectory;
            HashKey = hashKey;
            CapturePhotos = capturePhotos;
            CaptureConnections = CaptureConnections;
            DownloadPhotoCheck = downloadPhotoCheck;
        }

        /// <summary>
        /// Start list download.
        /// </summary>
        public void Download()
        {
            using (Logger listLog = new Logger(StoreDirectory, "_list"))
            {
                try
                {
                    listLog.Log("Started.");
                    String line = "";
                    StreamReader file = new StreamReader(ListFileName);
                    while ((line = file.ReadLine()) != null)
                    {
                        if (!Directory.Exists(Path.Combine(StoreDirectory, line.Trim())))
                        {
                            listLog.Log(String.Format("Processing {0}.", line.Trim()));
                            using (ProfileDownloader d = new ProfileDownloader(line.Trim(), StoreDirectory, HashKey, CapturePhotos, CaptureConnections, DownloadPhotoCheck))
                            {
                                d.Download();
                            }
                        }
                        else
                        {
                            listLog.Log(String.Format("Skipped {0}.", line.Trim()));
                        }
                    }
                    listLog.Log("Done.");
                }
                catch(Exception exc)
                {
                    listLog.Log(String.Format("Error: {0}", exc?.Message));
                }
            }
        }
    }
}
