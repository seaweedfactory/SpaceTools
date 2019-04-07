using Newtonsoft.Json;
using SpaceTools.Data;
using SpaceTools.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpaceTools.Tools.LocationCrawl
{
    /// <summary>
    /// Crawl a directory structure of profiles, parsing connections for location information.
    /// Write location information to JSON file.
    /// </summary>
    public class LocationCrawler
    {
        /// <summary>
        /// Hashkey for API calls.
        /// </summary>
        private String HashKey { get; set; }

        /// <summary>
        /// Directory in which profiles are stored.
        /// </summary>
        private String StoreDirectory { get; set; }

        /// <summary>
        /// Crawl a directory structure of profiles, parsing connections for location information.
        /// Write location information to JSON file.
        /// </summary>
        /// <param name="storeDirectory">Directory in which profiles are stored.</param>
        /// <param name="hashKey">Hashkey for API calls.</param>
        public LocationCrawler(
            String storeDirectory,
            String hashKey)
        {
            StoreDirectory = storeDirectory;
            HashKey = hashKey;
        }

        /// <summary>
        /// Start crawl.
        /// </summary>
        public void Crawl()
        {
            //Ensure HTTPS will work correctly
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            List<ProfileLocationRecord> records = new List<ProfileLocationRecord>();

            using (Logger log = new Logger(StoreDirectory, "locationDownload"))
            {
                //Load existing usernames
                try
                {
                    records = JsonConvert.DeserializeObject<List<ProfileLocationRecord>>(File.ReadAllText(Path.Combine(StoreDirectory, "locations.json")));
                    if (records == null)
                    {
                        log.Log("Could not load locations file.");
                        records = new List<ProfileLocationRecord>();
                    }
                }
                catch (Exception e)
                {
                    log.Log("Could not load locations file.");
                    records = new List<ProfileLocationRecord>();
                }

                String[] profileFiles = Directory.GetFiles(StoreDirectory, @"*.profile.json", SearchOption.AllDirectories);
                foreach (String profileFileName in profileFiles)
                {
                    if (!String.IsNullOrEmpty(profileFileName))
                    {
                        try
                        {
                            Profile parentProfile = JsonConvert.DeserializeObject<Profile>(File.ReadAllText(profileFileName));
                            if (parentProfile == null)
                            {
                                log.Log(String.Format(@"Empty Profile: {0}", profileFileName));
                            }
                            else
                            {
                                log.Log(String.Format("Processing {0}", parentProfile?.UserName));

                                if (records.Where(x => String.Equals(x.UserName, parentProfile.UserName)).FirstOrDefault() == null)
                                {
                                    //Add parent record
                                    records.Add(new ProfileLocationRecord()
                                    {
                                        UserName = parentProfile.UserName,
                                        PersonalName = parentProfile.PersonalName,
                                        Location = parentProfile.LocationDescription,
                                        ConnectedFromLocation = null,
                                        Error = null,
                                    });

                                    log.Log(String.Format("Added {0}", parentProfile.UserName));
                                }

                                //Process all connection records
                                int connectionCount = 0;
                                int connectionTotal = parentProfile.Connections != null ? parentProfile.Connections.Count : 0;
                                foreach (ConnectionEntry connection in parentProfile.Connections)
                                {
                                    connectionCount++;
                                    try
                                    {
                                        //Check if exists
                                        if (records.Where(x => String.Equals(x.UserName, connection.UserName)).FirstOrDefault() == null)
                                        {
                                            //Get the location, then add the record
                                            String locationDescription = CrawlUtil.GetUserLocation(connection.UserName);
                                            Thread.Sleep(20);
                                            records.Add(new ProfileLocationRecord()
                                            {
                                                UserName = connection.UserName,
                                                PersonalName = connection.PersonalName,
                                                Location = locationDescription,
                                                ConnectedFromLocation = parentProfile.LocationDescription,
                                                Error = null,
                                            });

                                            log.Log(String.Format(@"({1}/{2})Added {0}", connection.UserName, connectionCount, connectionTotal));
                                        }
                                        else
                                        {
                                            //log.Log(String.Format("({1}/{2})Skipped {0}", connection.UserName, connectionCount, connectionTotal));
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        log.Log(String.Format("Error: {0}", e?.Message));
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(String.Format(@"Error loading profile: {0}", profileFileName));
                            return;
                        }
                    }

                    //Update location file
                    try
                    {
                        File.WriteAllText(
                                    Path.Combine(StoreDirectory, "locations.json"),
                                    JsonConvert.SerializeObject(records, Formatting.Indented));
                    }
                    catch (Exception e)
                    {
                        log.Log(String.Format(@"Error Saving Locations: {0}", e?.Message));
                        return;
                    }

                }
            }
        }
    }
}
