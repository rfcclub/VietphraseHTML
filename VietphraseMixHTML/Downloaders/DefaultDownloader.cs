using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace VietphraseMixHTML.Downloaders
{
    public class DefaultDownloader : IDownloader
    {
        public WebClient Client { get;set;}
        public WebBrowser Browser { get; set; }
        public int RetryCount { get;set; }

        public DefaultDownloader()
        {
            RetryCount = 1;
        }

        public virtual string Download(string url)
        {
            return ClientDownload(url);
            
        }

        public  string ClientDownload(string url)
        {
            string htmlContent = null;
            Utility.TryActionHelper(delegate() { htmlContent = Client.DownloadString(url); }, RetryCount);
            return htmlContent;
        }

        public virtual HtmlAgilityPack.HtmlDocument GetHtmlDocument(string url)
        {
            var htmlDocument = new HtmlAgilityPack.HtmlDocument();
            var request = (HttpWebRequest) WebRequest.Create(url);

            try
            {
                htmlDocument.Load(((HttpWebResponse) request.GetResponse()).GetResponseStream());
            }
            catch (Exception e)
            {
                htmlDocument = null;
            }
            return htmlDocument;
        }
    }
}
