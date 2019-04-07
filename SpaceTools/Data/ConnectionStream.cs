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
    /// Handles request to connections API.
    /// </summary>
    public class ConnectionStream
    {
        public List<ConnectionEntry> Connections { get; private set; }

        public String UserName { get; private set; }
        public String HashKey { get; private set; }
        public int DelayBetweenAPIRequests { get; private set; }
        public ConnectionDirection Direction { get; private set; }

        public ConnectionStream(String userName, String hashKey, ConnectionDirection direction, int delayBetweenAPIRequests)
        {
            Connections = new List<ConnectionEntry>();
            UserName = userName;
            HashKey = hashKey;
            Direction = direction;
            DelayBetweenAPIRequests = delayBetweenAPIRequests;
        }

        public HttpWebRequest BuildRequest(ConnectionDirection direction, int startingIndex = 0)
        {
            String directionURLToken = "";
            switch (direction)
            {
                case ConnectionDirection.Unknown:
                    return null;

                case ConnectionDirection.Out:
                    directionURLToken = "out";
                    break;

                case ConnectionDirection.In:
                    directionURLToken = "in";
                    break;
            }

            HttpWebRequest rq = (HttpWebRequest)WebRequest
                .Create(String.Format(@"https://myspace.com/ajax/{0}/connections/{1}", UserName, directionURLToken));

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

        public ConnectionStreamResponse RequestConnectionStream(int nextStart)
        {
            ConnectionStreamResponse fullResponse = new ConnectionStreamResponse();
            try
            {
                HttpWebRequest request = BuildRequest(Direction, nextStart);
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

                    var connectionNodes = doc.DocumentNode.SelectNodes(String.Format("//ul[@id='profileGrid']//li"));
                    foreach (var connectionNode in connectionNodes)
                    {
                        ConnectionEntry entry = new ConnectionEntry();
                        entry.Direction = Direction;
                        entry.UserURL = connectionNode.SelectSingleNode("div//a")?.Attributes["href"]?.Value;
                        if (!String.IsNullOrEmpty(entry.UserURL) && entry.UserURL.StartsWith(@"/"))
                        {
                            entry.UserName = entry.UserURL.Replace(@"/", "");
                            entry.UserURL = string.Format(@"https://myspace.com{0}", entry.UserURL);
                        }
                        entry.ThumbnailURL = connectionNode.SelectSingleNode("div//a//img")?.Attributes["src"]?.Value;
                        entry.PersonalName = connectionNode.SelectSingleNode("div//a//div//h6")?.InnerHtml;
                        entry.ProfileID = connectionNode.SelectSingleNode(String.Format("//div[@data-title='{0}']", entry.PersonalName))?.Attributes["data-id"]?.Value;
                        entry.ArtistID = connectionNode.SelectSingleNode(String.Format("//div[@data-title='{0}']", entry.PersonalName))?.Attributes["data-artist-id"]?.Value;
                        
                        if (!String.IsNullOrEmpty(entry.UserURL))
                        {
                            fullResponse.ConnectionEntries.Add(entry);
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
            ConnectionStreamResponse r = new ConnectionStreamResponse()
            {
                NextStart = 0
            };
            while (!r.EndOfConnections)
            {
                r = RequestConnectionStream(r.NextStart);
                if (r == null || !(String.IsNullOrEmpty(r.Error)))
                {
                    //return on error
                    return;
                }
                else
                {
                    Connections.AddRange(r.ConnectionEntries);
                }

                if (!r.EndOfConnections)
                {
                    Thread.Sleep(CrawlUtil.GetVariableDelay(DelayBetweenAPIRequests));
                }
            }
        }
    }

    public class ConnectionStreamResponse
    {
        public String Error { get; set; }
        public int NextStart { get; set; }
        public bool EndOfConnections
        {
            get
            {
                return NextStart == -1;
            }
        }
        public List<ConnectionEntry> ConnectionEntries { get; private set; } = new List<ConnectionEntry>();
    }
}
