using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VietphraseMixHTML.Downloaders
{
    public class GoogleTranslateDownloader : DefaultDownloader
    {
        public override string Download(string url)
        {
            string translateUrl = @"http://google.com/gwt/n?noimg=1&source=wax&u=";
            translateUrl += url;
            return ClientDownload(translateUrl);
        }
    }
}
