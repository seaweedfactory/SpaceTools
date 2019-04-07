using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SpaceTools.Utility;
using SpaceTools.Data;

namespace SpaceTools.Tools.Parser
{ 
    /// <summary>
    /// Handles parsing of a profile.
    /// </summary>
    public class ProfileParser
    {
        /// <summary>
        /// Hashkey to use for API calls.
        /// </summary>
        public String HashKey { get; private set; }

        /// <summary>
        /// Profile to fill.
        /// </summary>
        public Profile Profile { get; private set; }

        /// <summary>
        /// Delay between API calls. This is a base value which will be slightly randomized.
        /// </summary>
        public int DelayBetweenAPIRequests { get; private set; }

        /// <summary>
        /// Delay between page requests. This is a base value which will be slightly randomized.
        /// </summary>
        public int DelayBetweenPages { get; private set; }

        /// <summary>
        /// Should connections be parsed?
        /// </summary>
        public bool CaptureConnections { get; set; }

        /// <summary>
        /// Parse the given profile page.
        /// </summary>
        /// <param name="logger">Logging manager.</param>
        /// <param name="hashKey">Hashkey to use for API calls.</param>
        /// <param name="userName">Profile name to parse.</param>
        /// <param name="delayBetweenPages">Delay between page requests. This is a base value which will be slightly randomized.</param>
        /// <param name="delayBetweenAPIRequests">Delay between API calls. This is a base value which will be slightly randomized.</param>
        /// <param name="captureConnections">Should connections be parsed?</param>
        /// <returns>Parsed profile.</returns>
        public Profile Parse(
            ILog logger, 
            String hashKey, 
            String userName, 
            int delayBetweenPages, 
            int delayBetweenAPIRequests, 
            bool captureConnections)
        {
            HashKey = hashKey;
            DelayBetweenPages = delayBetweenPages;
            DelayBetweenAPIRequests = delayBetweenAPIRequests;
            CaptureConnections = captureConnections;
            Profile = new Profile();
            Profile.Captured = DateTime.Now;
            Profile.CapturedConnections = captureConnections;
            ParseProfilePage(userName);
            Thread.Sleep(CrawlUtil.GetVariableDelay(DelayBetweenPages));
            if (!Profile.IsPrivate)
            {
                ParseBio();
                Thread.Sleep(CrawlUtil.GetVariableDelay(DelayBetweenPages));
                logger.Log(String.Format("Parsing Photos: UserName={0}", userName));
                ParsePhotos();
                Thread.Sleep(CrawlUtil.GetVariableDelay(DelayBetweenPages));
                if (CaptureConnections)
                {
                    logger.Log(String.Format("Parsing Connections Out: UserName={0}", userName));
                    ParseConnections(ConnectionDirection.Out);
                    Thread.Sleep(CrawlUtil.GetVariableDelay(DelayBetweenPages));
                    logger.Log(String.Format("Parsing Connections In: UserName={0}", userName));
                    ParseConnections(ConnectionDirection.In);
                    Thread.Sleep(CrawlUtil.GetVariableDelay(DelayBetweenPages));
                }
                logger.Log(String.Format("Parsing Songs: UserName={0}", userName));
                ParseSongs();
                Thread.Sleep(CrawlUtil.GetVariableDelay(DelayBetweenPages));
                logger.Log(String.Format("Parsing Videos: Videos={0}", userName));
                ParseVideos();
                Thread.Sleep(CrawlUtil.GetVariableDelay(DelayBetweenPages));
            }
            else
            {
                logger.Log(String.Format("Private profile: UserName={0}", userName));
            }
            return Profile;
        }

        /// <summary>
        /// Parse root profile page.
        /// </summary>
        /// <param name="userName">Profile page to parse.</param>
        private void ParseProfilePage(String userName)
        {
            String profileURL = String.Format(@"https://myspace.com/{0}", userName);
            var doc = new HtmlAgilityPack.HtmlDocument();
            HtmlAgilityPack.HtmlNode.ElementsFlags["br"] = HtmlAgilityPack.HtmlElementFlag.Empty;
            doc.OptionWriteEmptyNodes = true;

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var webRequest = HttpWebRequest.Create(profileURL);
                ((HttpWebRequest)webRequest).UserAgent = CrawlUtil.GetUserAgent();
                Stream stream = webRequest.GetResponse().GetResponseStream();
                doc.Load(stream);
                stream.Close();

                Profile.URL = String.Format(@"https://myspace.com/{0}", userName);
                Profile.UserName = userName;
                Profile.ProfileThumbnailImageURL = doc.DocumentNode.SelectSingleNode(@"//a[@id='profileImage']//img")?.Attributes["src"]?.Value;
                Profile.ProfileImageURL = !String.IsNullOrEmpty(Profile.ProfileThumbnailImageURL) ? CrawlUtil.ModifyUriFileName(Profile.ProfileThumbnailImageURL, x => "600x600") : null;
                Profile.ProfileID = doc.DocumentNode.SelectSingleNode(@"//div[@class='connectButton notReversed tooltips']")?.Attributes["data-id"]?.Value;
                String privateFlag = doc.DocumentNode.SelectSingleNode(@"//div[@class='connectButton notReversed tooltips']")?.Attributes["data-is-private"]?.Value;
                Profile.IsPrivate = privateFlag != null && privateFlag.ToLower().Equals("true");
                Profile.PersonalName = doc.DocumentNode.SelectSingleNode(@"//div[@class='connectButton notReversed tooltips']")?.Attributes["data-title"]?.Value;
                Profile.LocationDescription = doc.DocumentNode.SelectSingleNode(@"//div[@id='profileDetails']//div[@id='locAndWeb']//div[@class='location_white location ']")?.Attributes["data-display-text"]?.Value;
                Profile.Website = doc.DocumentNode.SelectSingleNode(@"//div[@id='profileDetails']//div[@id='locAndWeb']//div[@class='ribbon_white website ']//a")?.InnerText;
                Profile.OutConnectionTotal = doc.DocumentNode.SelectSingleNode(String.Format(@"//div[@id='profileDetails']//div[@id='connectionsCount']//a[@href='/{0}/connections/out']//span", Profile.UserName))?.InnerText;
                Profile.InConnectionTotal = doc.DocumentNode.SelectSingleNode(String.Format(@"//div[@id='profileDetails']//div[@id='connectionsCount']//a[@href='/{0}/connections/in']//span", Profile.UserName))?.InnerText;

                if (!Profile.IsPrivate)
                {
                    var top8FriendsNode = doc.DocumentNode.SelectNodes(@"//div[@class='friendsWrapper']//ul//li//a");
                    if (top8FriendsNode != null)
                    {
                        foreach (var friendNode in top8FriendsNode)
                        {
                            Top8FriendEntry friendEntry = new Top8FriendEntry();
                            friendEntry.UserURL = friendNode?.Attributes["href"]?.Value;
                            if (!String.IsNullOrEmpty(friendEntry.UserURL) && friendEntry.UserURL.StartsWith("/"))
                            {
                                friendEntry.UserURL = string.Format(@"https://myspace.com{0}", friendEntry.UserURL);
                            }
                            friendEntry.ProfileID = friendNode?.Attributes["data-profileid"]?.Value;
                            friendEntry.ThumbnailURL = friendNode?.Attributes["data-image-url"]?.Value;
                            friendEntry.UserName = friendNode?.Attributes["data-title"]?.Value;
                            Profile.Top8Friends.Add(friendEntry);
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// Parse biography information.
        /// </summary>
        private void ParseBio()
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            HtmlAgilityPack.HtmlNode.ElementsFlags["br"] = HtmlAgilityPack.HtmlElementFlag.Empty;
            doc.OptionWriteEmptyNodes = true;

            try
            {
                var webRequest = HttpWebRequest.Create(String.Format(@"https://myspace.com/{0}/bio", Profile.UserName));
                ((HttpWebRequest)webRequest).UserAgent = CrawlUtil.GetUserAgent();
                Stream stream = webRequest.GetResponse().GetResponseStream();
                doc.Load(stream);
                stream.Close();

                Profile.Biography = doc.DocumentNode.SelectSingleNode(@"//div[@class='mainBio']//div[@class='bioColumns']//div")?.InnerHtml?.ToString()?.Trim();
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// Parse photostream information.
        /// </summary>
        private void ParsePhotos()
        {
            try
            {
                PhotoStream ps = new PhotoStream(Profile.UserName, HashKey, DelayBetweenAPIRequests);
                ps.Read();
                if (ps.Photos != null && ps.Photos.Count > 0)
                {
                    Profile.Photos.AddRange(ps.Photos);
                }
            }
            catch(Exception e)
            {

            }
        }

        /// <summary>
        /// Parse connection information.
        /// </summary>
        /// <param name="direction">In or out connections.</param>
        private void ParseConnections(ConnectionDirection direction)
        {
            try
            {
                ConnectionStream cs = new ConnectionStream(Profile.UserName, HashKey, direction, DelayBetweenAPIRequests);
                cs.Read();
                if (cs.Connections != null && cs.Connections.Count > 0)
                {
                    Profile.Connections.AddRange(cs.Connections);
                }
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// Parse song data.
        /// </summary>
        private void ParseSongs()
        {
            try
            {
                SongStream ss = new SongStream(Profile.UserName, HashKey, DelayBetweenAPIRequests);
                ss.Read();
                if (ss.Songs != null && ss.Songs.Count > 0)
                {
                    Profile.Songs.AddRange(ss.Songs);
                }
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// Parse video data.
        /// </summary>
        private void ParseVideos()
        {
            try
            {
                VideoStream vs = new VideoStream(Profile.UserName, HashKey, DelayBetweenAPIRequests);
                vs.Read();
                if (vs.Videos != null && vs.Videos.Count > 0)
                {
                    Profile.Videos.AddRange(vs.Videos);
                }
            }
            catch (Exception e)
            {

            }
        }
    }
}
