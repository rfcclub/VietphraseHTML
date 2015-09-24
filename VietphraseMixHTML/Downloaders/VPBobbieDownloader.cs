using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VietphraseMixHTML.Downloaders
{
    public class VPBobbieDownloader : DefaultDownloader
    {
        public override string Download(string url)
        {
            string htmlContent = null;
            string processUrl = url;
            string translateUrl = @"http://vp.botbie.com/";
            if (processUrl.StartsWith(@"http://")) processUrl = processUrl.Substring(7);
            translateUrl += processUrl;
            htmlContent = ClientDownload(translateUrl);
            htmlContent = "VPBOBBIE" + htmlContent;
            return htmlContent;
        }
    }
}
