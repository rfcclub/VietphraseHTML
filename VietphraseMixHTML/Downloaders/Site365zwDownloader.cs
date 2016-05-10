using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace VietphraseMixHTML.Downloaders
{
    public class Site365zwDownloader : DefaultDownloader
    {
        public override string Download(string url)
        {
            string address = null;
            HtmlAgilityPack.HtmlDocument htmlDocument = GetHtmlDocument(url);
            if (htmlDocument == null) return null;
            foreach (HtmlNode link in htmlDocument.DocumentNode.SelectNodes("//script"))
            {
                // 365zw
                if (link.InnerText.StartsWith("outputTxt"))
                {
                    string linkText = link.InnerText;
                    address = linkText.Substring(linkText.IndexOf("\"") + 1);
                    address = @"http://res.365zw.com/novel" +
                              address.Substring(0, address.LastIndexOf("\""));
                    break;
                }
            }
            if (address == null) return null;
            return ClientDownload(address);
        }
    }
}
