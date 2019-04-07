using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SpaceTools.Utility
{
    /// <summary>
    /// Utility functions for crawling data.
    /// </summary>
    public class CrawlUtil
    {
        /// <summary>
        /// Download a file from a URL.
        /// </summary>
        /// <param name="url">URL of file.</param>
        /// <param name="name">Name to write file to disk.</param>
        /// <returns>true if successful.</returns>
        public static bool GetFile(String url, String name)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.Credentials = new NetworkCredential("username", "password", "domain");
                    client.Headers.Add(HttpRequestHeader.UserAgent, CrawlUtil.GetUserAgent());
                    client.DownloadFile(url, name);
                    return true;
                }
            }
            catch(Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// Download a file from a URL asynchronously.
        /// </summary>
        /// <param name="url">URL of file.</param>
        /// <param name="name">Name to write file to disk.</param>
        /// <param name="timeout">Timeout period after file request.</param>
        /// <returns>true if successful.</returns>
        public static async Task<bool> GetFileAsync(String url, String name, int timeout)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var client = new WebDownload(timeout))
                    {
                        client.Credentials = new NetworkCredential("username", "password", "domain");
                        client.Headers.Add(HttpRequestHeader.UserAgent, CrawlUtil.GetUserAgent());
                        client.DownloadFile(url, name);
                        return true;
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
            });
            return false;
        }

        /// <summary>
        /// Download a file from a URL.
        /// </summary>
        /// <param name="url">URL of file.</param>
        /// <param name="name">Name to write file to disk.</param>
        /// <param name="timeout">Timeout period after file request.</param>
        /// <returns>true if successful.</returns>
        public static bool GetFile(String url, String name, int timeout)
        {
            try
            {
                using (var client = new WebDownload(timeout))
                {
                    client.Credentials = new NetworkCredential("username", "password", "domain");
                    client.Headers.Add(HttpRequestHeader.UserAgent, CrawlUtil.GetUserAgent());
                    client.DownloadFile(url, name);
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// Changes the file name of the <paramref name="uri"/> using the given <paramref name="modifier"/>
        /// </summary>
        /// <param name="uri">A relative or absolute uri</param>
        /// <param name="modifier">A function to apply to the filename</param>
        /// <returns>The modified uri</returns>
        /// <remarks>
        /// Usage:
        /// return ModifyUriFileName(uri, s => s + "_fixed");
        /// </remarks>
        public static string ModifyUriFileName(string uri, Func<string, string> modifier)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentNullException("uri");
            }

            var fileName = Path.GetFileNameWithoutExtension(uri);
            var extension = Path.GetExtension(uri);
            var path = uri.Substring(0, uri.LastIndexOf('/') + 1);

            return string.Format("{0}{1}{2}", path, modifier(fileName), extension);
        }

        /// <summary>
        /// Get a user agent string for webrequests.
        /// </summary>
        /// <returns>User agent string.</returns>
        public static String GetUserAgent()
        {
            return @"Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:62.0) Gecko/20100101 Firefox/62.0";
        }

        /// <summary>
        /// Vary a delay by up to x% in either direction.
        /// </summary>
        /// <param name="baseDelay">Base delay.</param>
        /// <param name="variance">Percent to vary delay.</param>
        /// <returns></returns>
        public static int GetVariableDelay(int baseDelay, int variance = 10)
        {
            Random rnd = new Random();
            return baseDelay + rnd.Next((baseDelay/variance) * 2) - (baseDelay / variance);
        }

        /// <summary>
        /// Get the user's location from a profile.
        /// </summary>
        /// <param name="userName">Profile to parse.</param>
        /// <returns>Location string, if available.</returns>
        /// <remarks>Shortcut function to avoid parsing entire profile.</remarks>
        public static String GetUserLocation(String userName)
        {
            String profileURL = String.Format(@"https://myspace.com/{0}", userName);
            var doc = new HtmlAgilityPack.HtmlDocument();
            HtmlAgilityPack.HtmlNode.ElementsFlags["br"] = HtmlAgilityPack.HtmlElementFlag.Empty;
            doc.OptionWriteEmptyNodes = true;

            try
            {
                var webRequest = HttpWebRequest.Create(profileURL);
                ((HttpWebRequest)webRequest).UserAgent = CrawlUtil.GetUserAgent();
                Stream stream = webRequest.GetResponse().GetResponseStream();
                doc.Load(stream);
                stream.Close();

                return doc.DocumentNode.SelectSingleNode(@"//div[@id='profileDetails']//div[@id='locAndWeb']//div[@class='location_white location ']")?.Attributes["data-display-text"]?.Value;
            }
            catch (Exception e)
            {

            }

            return null;
        }
    }
}
