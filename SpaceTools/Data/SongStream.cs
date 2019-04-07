using Newtonsoft.Json.Linq;
using SpaceTools.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpaceTools.Data
{
    /// <summary>
    /// Handles request to song API.
    /// </summary>
    public class SongStream
    {
        public List<SongEntry> Songs { get; private set; }

        public String UserName { get; private set; }
        public String HashKey { get; private set; }
        public int DelayBetweenAPIRequests { get; private set; }
        
        public SongStream(String userName, String hashKey, int delayBetweenAPIRequests)
        {
            Songs = new List<SongEntry>();
            UserName = userName;
            HashKey = hashKey;
            DelayBetweenAPIRequests = delayBetweenAPIRequests;
        }
        
        public void Read()
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            HtmlAgilityPack.HtmlNode.ElementsFlags["br"] = HtmlAgilityPack.HtmlElementFlag.Empty;
            doc.OptionWriteEmptyNodes = true;

            try
            {
                var webRequest = HttpWebRequest.Create(String.Format(@"https://myspace.com/{0}/music/songs", UserName));
                ((HttpWebRequest)webRequest).UserAgent = CrawlUtil.GetUserAgent();
                Stream stream = webRequest.GetResponse().GetResponseStream();
                doc.Load(stream);
                stream.Close();

                var songsNode = doc.DocumentNode.SelectNodes(@"//button[@class='playBtn play_25 song']");
                if (songsNode != null)
                {
                    foreach (var songNode in songsNode)
                    {
                        SongEntry entry = new SongEntry();

                        #region Parse summary page
                        entry.SongID = songNode?.Attributes["data-song-id"]?.Value;
                        entry.SongTitle = songNode?.Attributes["data-title"]?.Value;
                        entry.SongURL = songNode?.Attributes["data-url"]?.Value;
                        if (!String.IsNullOrEmpty(entry.SongURL))
                        {
                            entry.SongURL = String.Format(@"https://myspace.com{0}", entry.SongURL);
                        }

                        entry.AlbumID = songNode?.Attributes["data-album-id"]?.Value;
                        entry.AlbumTitle = songNode?.Attributes["data-album-title"]?.Value;
                        entry.AlbumURL = songNode?.Attributes["data-album-url"]?.Value;
                        if (!String.IsNullOrEmpty(entry.AlbumURL))
                        {
                            entry.AlbumURL = String.Format(@"https://myspace.com{0}", entry.AlbumURL);
                        }

                        entry.ArtistID = songNode?.Attributes["data-artist-id"]?.Value;
                        entry.ArtistTitle = songNode?.Attributes["data-artist-name"]?.Value;
                        entry.ArtistURL = songNode?.Attributes["data-artist-url"]?.Value;
                        if (!String.IsNullOrEmpty(entry.ArtistURL))
                        {
                            entry.ArtistURL = String.Format(@"https://myspace.com{0}", entry.ArtistURL);
                        }

                        entry.DurationInSeconds = songNode?.Attributes["data-duration"]?.Value;
                        entry.VideoID = songNode?.Attributes["data-video-id"]?.Value;
                        entry.YoutubeID = songNode?.Attributes["data-youtube-id"]?.Value;
                        if (!String.IsNullOrEmpty(entry.YoutubeID))
                        {
                            entry.YoutubeURL = String.Format(@"https://www.youtube.com/watch?v={0}", entry.YoutubeID);
                        }

                        entry.ImageThumbnailURL = songNode?.Attributes["data-image-url"]?.Value;
                        entry.ImageURL = !String.IsNullOrEmpty(entry.ImageThumbnailURL) ? CrawlUtil.ModifyUriFileName(entry.ImageThumbnailURL, x => "full") : null;

                        entry.GenreID = songNode?.Attributes["data-genre-id"]?.Value;
                        entry.GenreName = songNode?.Attributes["data-genre-name"]?.Value;
                        entry.MediaID = songNode?.Attributes["data-media-id"]?.Value;
                        entry.MediaType = songNode?.Attributes["data-media-type"]?.Value;
                        entry.UID = songNode?.Attributes["data-uid"]?.Value;

                        String isPremiumFlag = songNode?.Attributes["data-is-premium"]?.Value;
                        entry.IsPremium = isPremiumFlag != null && isPremiumFlag.ToLower().Equals("true");

                        String isExplicitFlag = songNode?.Attributes["data-is-explicit"]?.Value;
                        entry.IsExplicit = isExplicitFlag != null && isExplicitFlag.ToLower().Equals("true");

                        String isFullLength = songNode?.Attributes["data-is-full-length"]?.Value;
                        entry.IsFullLength = isFullLength != null && isFullLength.ToLower().Equals("true");

                        String isAdsProhibited = songNode?.Attributes["data-ads-prohibited"]?.Value;
                        entry.IsAdsProhibited = isAdsProhibited != null && isAdsProhibited.ToLower().Equals("true");
                        #endregion

                        #region Parse detial page
                        if (!String.IsNullOrEmpty(entry.SongURL))
                        {
                            Thread.Sleep(CrawlUtil.GetVariableDelay(DelayBetweenAPIRequests));
                            try
                            {
                                var detailDoc = new HtmlAgilityPack.HtmlDocument();
                                HtmlAgilityPack.HtmlNode.ElementsFlags["br"] = HtmlAgilityPack.HtmlElementFlag.Empty;
                                detailDoc.OptionWriteEmptyNodes = true;

                                var webDetailRequest = HttpWebRequest.Create(entry.SongURL);
                                ((HttpWebRequest)webDetailRequest).UserAgent = CrawlUtil.GetUserAgent();
                                Stream detailStream = webDetailRequest.GetResponse().GetResponseStream();
                                detailDoc.Load(detailStream);
                                detailStream.Close();

                                var playsNodes = detailDoc.DocumentNode.SelectNodes(@"//div[@class='plays']");
                                if (playsNodes != null
                                && playsNodes.Count >= 2
                                && String.Equals(playsNodes[0]?.InnerText, "PLAYS", StringComparison.OrdinalIgnoreCase))
                                {
                                    entry.PlayCount = playsNodes[1]?.InnerText;
                                }

                                var asideNodes = detailDoc.DocumentNode.SelectNodes(@"//aside[@class='dotted top']");
                                if (asideNodes != null
                                && asideNodes.Count >= 1
                                && asideNodes[0]?.InnerText != null
                                && asideNodes[0].InnerText.Contains("Length"))
                                {
                                    var songDetailItemNodesDt = asideNodes[0].SelectNodes("//dt");
                                    var songDetailItemNodesDd = asideNodes[0].SelectNodes("//dd");
                                    if (songDetailItemNodesDt != null
                                    && songDetailItemNodesDt.Count > 0
                                    && songDetailItemNodesDd != null
                                    && songDetailItemNodesDd.Count > 0
                                    && songDetailItemNodesDt.Count == songDetailItemNodesDd.Count)
                                    {
                                        Dictionary<String, int> tableIndex = new Dictionary<string, int>();
                                        int songDetailItemDtCount = -1;
                                        foreach (var item in songDetailItemNodesDt)
                                        {
                                            songDetailItemDtCount++;
                                            tableIndex.Add(item.InnerText, songDetailItemDtCount);
                                        }

                                        if (tableIndex.ContainsKey("Label")
                                        && songDetailItemNodesDd.Count >= tableIndex["Label"])
                                        {
                                            int index = tableIndex["Label"];
                                            entry.Label = songDetailItemNodesDd[index]?.InnerText;
                                        }

                                        if (tableIndex.ContainsKey("Release")
                                        && songDetailItemNodesDd.Count >= tableIndex["Release"])
                                        {
                                            int index = tableIndex["Release"];
                                            entry.ReleaseDate = songDetailItemNodesDd[index]?.InnerText;
                                        }
                                    }
                                }
                            }
                            catch (Exception e2)
                            {

                            }
                        }
                        #endregion

                        if (!String.IsNullOrEmpty(entry.SongID))
                        {
                            Songs.Add(entry);
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }
    }
}
