using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace VietphraseMixHTML.Downloaders
{
    public class UukanshuDownloader : DefaultDownloader
    {
        public override string Download(string url)
        {

            string htmlContent = null;
            HtmlAgilityPack.HtmlDocument htmlDocument = GetHtmlDocument(url);
            if (htmlDocument == null) return null;

            Client.Encoding = Encoding.GetEncoding("GB18030");
            HtmlNode htmlNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='contentbox']");
            if (htmlNode != null) htmlContent = htmlNode.InnerHtml;
            return htmlContent;
        }
    }
}
