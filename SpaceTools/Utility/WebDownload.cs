using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpaceTools.Utility
{
    /// <summary>
    /// File download utility.
    /// </summary>
    /// <remarks>As a component to allow for easy, reliable use.</remarks>
    public class WebDownload : WebClient
    {
        /// <summary>
        /// Time in milliseconds
        /// </summary>
        public int Timeout { get; set; }

        public WebDownload() : this(60000) { }

        public WebDownload(int timeout)
        {
            this.Timeout = timeout;
        }

        /// <summary>
        /// Request a URL.
        /// </summary>
        /// <param name="address">URL to request.</param>
        /// <returns>Request object.</returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request != null)
            {
                request.Timeout = this.Timeout;
            }
            return request;
        }
    }
}
