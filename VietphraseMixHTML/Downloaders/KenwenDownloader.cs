using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VietphraseMixHTML.Downloaders
{
    public class KenwenDownloader : DefaultDownloader
    {
        public override string Download(string url)
        {
            String realUrl = url.Replace(".html", ".txt");
            return ClientDownload(realUrl);
        }
    }
}
