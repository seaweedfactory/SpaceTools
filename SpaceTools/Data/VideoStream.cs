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
    /// Handles request to video API.
    /// </summary>
    public class VideoStream
    {
        public List<VideoEntry> Videos { get; private set; }

        public String UserName { get; private set; }
        public String HashKey { get; private set; }
        public int DelayBetweenAPIRequests { get; private set; }

        public VideoStream(String userName, String hashKey, int delayBetweenAPIRequests)
        {
            Videos = new List<VideoEntry>();
            UserName = userName;
            HashKey = hashKey;
            DelayBetweenAPIRequests = delayBetweenAPIRequests;
        }

        public HttpWebRequest BuildRequest(int startingIndex = 0)
        {
            HttpWebRequest rq = (HttpWebRequest)WebRequest
                .Create(String.Format(@"https://myspace.com/ajax/{0}/videos", UserName));

            rq.UserAgent = CrawlUtil.GetUserAgent();
            rq.Host = "myspace.com";
            rq.Method = "POST";
            rq.Accept = @"application / json, text / javascript, */*; q=0.01";
            rq.ContentType = @"application/x-www-form-urlencoded; charset=UTF-8";
            rq.Headers.Add(@"Hash", HashKey);

            var postData = String.Format("start={0}", startingIndex);
            var data = Encoding.ASCII.GetBytes(postData);
            using (Stream s = rq.GetRequestStream())
            {
                s.Write(data, 0, data.Length);
            }

            return rq;
        }

        public VideoStreamResponse RequestVideoStream(int nextStart)
        {
            VideoStreamResponse fullResponse = new VideoStreamResponse();
            try
            {
                HttpWebRequest request = BuildRequest(nextStart);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Encoding responseEncoding = Encoding.GetEncoding(response.CharacterSet);
                String result = "";
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), responseEncoding))
                {
                    result = sr.ReadToEnd();
                    JObject model = JObject.Parse(result);

                    fullResponse.NextStart = (int)model["nextStart"];
                    
                    String htmlDocument = (String)model["html"];

                    var doc = new HtmlAgilityPack.HtmlDocument();
                    HtmlAgilityPack.HtmlNode.ElementsFlags["br"] = HtmlAgilityPack.HtmlElementFlag.Empty;
                    doc.OptionWriteEmptyNodes = true;

                    doc.LoadHtml(htmlDocument);

                    var videoNodes = doc.DocumentNode.SelectNodes("//ul[@class='grid videoItems infinite']//li");
                    if (videoNodes != null)
                    {
                        foreach (var videoNode in videoNodes)
                        {
                            VideoEntry entry = new VideoEntry();

                            var videoDetailNode = videoNode.SelectSingleNode(@"div//button[@class='playBtn play_25 video']");
                            entry.MediaType = videoDetailNode?.Attributes["data-media-type"]?.Value;
                            entry.MediaID = videoDetailNode?.Attributes["data-media-id"]?.Value;
                            entry.MediaReleaseID = videoDetailNode?.Attributes["data-media-release-id"]?.Value;
                            entry.EntityKey = videoDetailNode?.Attributes["data-entity-key"]?.Value;
                            entry.Title = videoDetailNode?.Attributes["data-title"]?.Value;
                            entry.Description = videoDetailNode?.Attributes["data-description"]?.Value;
                            entry.DurationInSeconds = videoDetailNode?.Attributes["data-duration"]?.Value;
                            entry.StreamURL = videoDetailNode?.Attributes["data-stream-url"]?.Value;
                            entry.ArtistID = videoDetailNode?.Attributes["data-artist-id"]?.Value;
                            entry.ArtistName = videoDetailNode?.Attributes["data-artist-name"]?.Value;
                            entry.ImageURL = videoDetailNode?.Attributes["data-image-url"]?.Value;
                            entry.UID = videoDetailNode?.Attributes["data-uid"]?.Value;
                            entry.DetailURL = videoDetailNode?.Attributes["data-detail-url"]?.Value;
                            if (!String.IsNullOrEmpty(entry.DetailURL))
                            {
                                entry.DetailURL = String.Format(@"https://myspace.com{0}", entry.DetailURL);
                            }
                            entry.ArtistURL = videoDetailNode?.Attributes["data-artist-url"]?.Value;
                            if (!String.IsNullOrEmpty(entry.ArtistURL))
                            {
                                entry.ArtistURL = String.Format(@"https://myspace.com{0}", entry.ArtistURL);
                            }
                            entry.EmbedURL = videoDetailNode?.Attributes["data-embed-url"]?.Value;
                            if (!String.IsNullOrEmpty(entry.EmbedURL))
                            {
                                entry.EmbedURL = String.Format(@"https://myspace.com{0}", entry.EmbedURL);
                            }
                            entry.EmbedType = videoDetailNode?.Attributes["data-embed-type"]?.Value;
                            entry.HLSStreamURL = videoDetailNode?.Attributes["data-hls-stream-url"]?.Value;
                            entry.ArtistUserName = videoDetailNode?.Attributes["data-artist-username"]?.Value;
                            entry.CustomLabel = videoDetailNode?.Attributes["data-custom-label"]?.Value;
                            entry.Category = videoDetailNode?.Attributes["data-category-name"]?.Value;
                            entry.MP4StreamURL = videoDetailNode?.Attributes["data-mp4-stream-url"]?.Value;

                            String isPremiumFlag = videoDetailNode?.Attributes["data-is-premium"]?.Value;
                            entry.IsPremium = isPremiumFlag != null && isPremiumFlag.ToLower().Equals("true");

                            String isExplicitFlag = videoDetailNode?.Attributes["data-is-explicit"]?.Value;
                            entry.IsExplicit = isExplicitFlag != null && isExplicitFlag.ToLower().Equals("true");

                            String isFullLength = videoDetailNode?.Attributes["data-is-full-length"]?.Value;
                            entry.IsFullLength = isFullLength != null && isFullLength.ToLower().Equals("true");

                            String isAdsProhibited = videoDetailNode?.Attributes["data-ads-prohibited"]?.Value;
                            entry.IsAdsProhibited = isAdsProhibited != null && isAdsProhibited.ToLower().Equals("true");

                            String isPrivate = videoDetailNode?.Attributes["data-private"]?.Value;
                            entry.IsPrivate = isPrivate != null && isPrivate.ToLower().Equals("true");

                            #region Parse play count
                            var italicNodes = videoNode?.SelectNodes("div//i");
                            var playLabelNode = italicNodes?.Where(x => x?.InnerText != null && x.InnerText.Contains("Plays"))?.FirstOrDefault();
                            if (playLabelNode?.NextSibling != null)
                            {
                                entry.PlayCount = playLabelNode.NextSibling?.InnerText;
                            }
                            #endregion

                            if (!String.IsNullOrEmpty(entry.Title))
                            {
                                fullResponse.VideoEntries.Add(entry);
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                fullResponse.Error = e.Message;
            }

            return fullResponse;
        }

        public void Read()
        {
            VideoStreamResponse r = new VideoStreamResponse()
            {
                NextStart = 0
            };
            while (!r.EndOfVideos)
            {
                r = RequestVideoStream(r.NextStart);
                if (r == null || !(String.IsNullOrEmpty(r.Error)))
                {
                    //return on error
                    return;
                }
                else
                {
                    Videos.AddRange(r.VideoEntries);
                }

                if (!r.EndOfVideos)
                {
                    Thread.Sleep(CrawlUtil.GetVariableDelay(DelayBetweenAPIRequests));
                }
            }
        }
    }

    public class VideoStreamResponse
    {
        public String Error { get; set; }
        public int NextStart { get; set; }
        public bool EndOfVideos
        {
            get
            {
                return NextStart == -1;
            }
        }
        public List<VideoEntry> VideoEntries { get; private set; } = new List<VideoEntry>();
    }
}
