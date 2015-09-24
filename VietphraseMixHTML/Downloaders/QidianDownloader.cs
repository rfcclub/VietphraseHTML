using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace VietphraseMixHTML.Downloaders
{
    public class QidianDownloader : DefaultDownloader
    {
        public override string Download(string url)
        {

            string htmlContent = null;
            HtmlAgilityPack.HtmlDocument htmlDocument = GetHtmlDocument(url);
            if (htmlDocument == null) return null;

            Client.Encoding = Encoding.GetEncoding("GB18030");
            foreach (HtmlNode link in htmlDocument.DocumentNode.SelectNodes("//script"))
            {
                // qidian
                HtmlAttribute att = link.Attributes["src"];
                if (att != null
                    && att.Value.IndexOf("files.qidian.com") > 0
                    && att.Value.EndsWith("txt"))
                {
                    Client.Encoding = Encoding.GetEncoding("GB18030");
                    htmlContent = ClientDownload(att.Value);
                    break;
                }
            }
            return htmlContent;
        }
    }
}
