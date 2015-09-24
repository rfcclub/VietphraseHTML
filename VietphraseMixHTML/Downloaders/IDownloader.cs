using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace VietphraseMixHTML.Downloaders
{
    public interface IDownloader
    {
        WebClient Client { get; set; }
        WebBrowser Browser { get; set; }

        string Download(string url);
    }
}
