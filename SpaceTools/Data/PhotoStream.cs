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
    /// Handles request to photos API.
    /// </summary>
    public class PhotoStream
    {
        public List<PhotoEntry> Photos { get; private set; }
        public String UserName { get; private set; }
        public String HashKey { get; private set; }
        public int DelayBetweenAPIRequests { get; private set; }

        public PhotoStream(String userName, String hashKey, int delayBetweenAPIRequests)
        {
            Photos = new List<PhotoEntry>();
            UserName = userName;
            HashKey = hashKey;
            DelayBetweenAPIRequests = delayBetweenAPIRequests;
        }

        public HttpWebRequest BuildRequest(String startingImageID = null)
        {
            HttpWebRequest rq = (HttpWebRequest)WebRequest
                .Create(String.Format(@"https://myspace.com/ajax/{0}/photosStream/", UserName));

            rq.UserAgent = CrawlUtil.GetUserAgent();
            rq.Host = "myspace.com";
            rq.Method = "POST";
            rq.Accept = @"application / json, text / javascript, */*; q=0.01";
            rq.ContentType = @"application/x-www-form-urlencoded; charset=UTF-8";
            rq.Headers.Add(@"Hash", HashKey);

            var postData = String.Format("lastImageId={0}", startingImageID);
            var data = Encoding.ASCII.GetBytes(postData);
            using (Stream s = rq.GetRequestStream())
            {
                if (!String.IsNullOrEmpty(startingImageID))
                {
                    s.Write(data, 0, data.Length);
                }
            }

            return rq;
        }

        public PhotoStreamResponse RequestPhotoStream(String startingImageID)
        {
            PhotoStreamResponse fullResponse = new PhotoStreamResponse();
            try
            {
                HttpWebRequest request = BuildRequest(startingImageID);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Encoding responseEncoding = Encoding.GetEncoding(response.CharacterSet);
                String result = "";
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), responseEncoding))
                {
                    result = sr.ReadToEnd();
                    JObject model = JObject.Parse(result);

                    fullResponse.EndOfPhotos = (bool)model["endOfPhotos"];

                    String htmlDocument = (String)model["view"];

                    var doc = new HtmlAgilityPack.HtmlDocument();
                    HtmlAgilityPack.HtmlNode.ElementsFlags["br"] = HtmlAgilityPack.HtmlElementFlag.Empty;
                    doc.OptionWriteEmptyNodes = true;

                    doc.LoadHtml(htmlDocument);

                    var photoNodes = doc.DocumentNode.SelectNodes(String.Format("//ul[@id='photosContainer']//li"));
                    if (photoNodes != null)
                    {
                        foreach (var photoNode in photoNodes)
                        {
                            PhotoEntry entry = new PhotoEntry();
                            entry.Caption = photoNode.SelectSingleNode("div//div//span[@class='photoCaption postText']")?.InnerText;
                            entry.ThumbnailImageURL = photoNode.SelectSingleNode("a//img")?.Attributes["src"]?.Value;
                            entry.FullImageURL = !String.IsNullOrEmpty(entry.ThumbnailImageURL) ? CrawlUtil.ModifyUriFileName(entry.ThumbnailImageURL, x => "full") : null;
                            entry.PhotoID = photoNode.Attributes["data-photoId"]?.Value;
                            entry.AlbumName = photoNode.SelectSingleNode("span[@itemprop='name']")?.InnerText;
                            entry.DetailPageURL = photoNode.SelectSingleNode("a")?.Attributes["content"]?.Value;

                            if (!String.IsNullOrEmpty(entry.DetailPageURL))
                            {
                                entry.DetailPageURL = String.Format(@"https://myspace.com{0}", entry.DetailPageURL);
                                ParseDetailPage(entry, entry.DetailPageURL);
                            }

                            if (!String.IsNullOrEmpty(entry.PhotoID))
                            {
                                fullResponse.PhotosEntries.Add(entry);
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                fullResponse.Error = e.Message;
            }

            if(fullResponse.PhotosEntries != null & fullResponse.PhotosEntries.Count > 0)
            {
                fullResponse.LastPhotoID = fullResponse.PhotosEntries[fullResponse.PhotosEntries.Count - 1]?.PhotoID;
            }

            return fullResponse;
        }

        public void Read()
        {
            PhotoStreamResponse r = new PhotoStreamResponse()
            {
                EndOfPhotos = false,
                LastPhotoID = null
            };

            while (!r.EndOfPhotos)
            {
                r = RequestPhotoStream(r.LastPhotoID);
                if (r == null || !(String.IsNullOrEmpty(r.Error)))
                {
                    //return on error
                    return;
                }
                else
                {
                    Photos.AddRange(r.PhotosEntries);
                }

                if(!r.EndOfPhotos)
                {
                    Thread.Sleep(CrawlUtil.GetVariableDelay(DelayBetweenAPIRequests));
                }
            }
        }

        public void ParseDetailPage(PhotoEntry photoEntry, String detailURL)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            HtmlAgilityPack.HtmlNode.ElementsFlags["br"] = HtmlAgilityPack.HtmlElementFlag.Empty;
            doc.OptionWriteEmptyNodes = true;

            try
            {
                var webRequest = HttpWebRequest.Create(detailURL);
                ((HttpWebRequest)webRequest).UserAgent = CrawlUtil.GetUserAgent();
                Stream stream = webRequest.GetResponse().GetResponseStream();
                doc.Load(stream);
                stream.Close();

                #region Parse photo properties
                var statsNode = doc.DocumentNode.SelectSingleNode("//div[@class='rr']//header[@class='stats']");
                photoEntry.LikesCount = statsNode.SelectSingleNode("a[@data-view='likes']//span")?.InnerText;
                photoEntry.ConnectsCount = statsNode.SelectSingleNode("a[@data-view='connects']//span")?.InnerText;
                photoEntry.CommentsCount = statsNode.SelectSingleNode("a[@data-view='comments']//span")?.InnerText;
                photoEntry.SharesCount = doc.DocumentNode.SelectSingleNode("//div[@class='genInfo ']//p[@class='stats']//span")?.InnerText;
                photoEntry.ConnectsEntityKey = doc.DocumentNode.SelectSingleNode("//div[@class='rr']")?.Attributes["data-connects-entity-key"]?.Value;
                #endregion

                #region Parse visible comments
                var commentNodes = doc.DocumentNode.SelectNodes("//ol//li");
                if (commentNodes != null)
                {
                    foreach (var commentNode in commentNodes)
                    {
                        PhotoCommentEntry entry = new PhotoCommentEntry();
                        entry.ProfileURL = commentNode.SelectSingleNode("div//div//div//a")?.Attributes["href"]?.Value;
                        entry.ThumbnailImageURL = commentNode.SelectSingleNode("a//img")?.Attributes["src"]?.Value;
                        if (!String.IsNullOrEmpty(entry.ProfileURL))
                        {
                            entry.ProfileURL = String.Format(@"https://myspace.com{0}", entry.ProfileURL);
                        }

                        entry.UserName = commentNode.SelectSingleNode("div//div//div//a")?.InnerText;
                        entry.CommentHTML = commentNode.SelectSingleNode("div//div//div//span")?.InnerHtml;
                        entry.Comment = commentNode.SelectSingleNode("div//div//div//span")?.InnerText;
                        entry.DateTimeUTC = commentNode.SelectSingleNode("div//div[@class='commentFooter']//time")?.Attributes["datetime"]?.Value;
                        entry.DateTimeDisplay = commentNode.SelectSingleNode("div//div[@class='commentFooter']//time")?.InnerText;

                        photoEntry.Comments.Add(entry);
                    }
                }
                #endregion
            }
            catch(Exception e)
            {

            }
        }
    }

    public class PhotoStreamResponse
    {
        public String Error { get; set; }
        public bool EndOfPhotos { get; set; }
        public String LastPhotoID { get; set; }
        public List<PhotoEntry> PhotosEntries { get; private set; } = new List<PhotoEntry>();
    }
}
