using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VietphraseMixHTML.Downloaders
{
    public class YuanchuangDownloader : DefaultDownloader
    {
        public override string Download(string url)
        {

            string htmlContent = null;
            String realUrl = url;
            //String realUrl = url.Replace(".html", ".txt").Replace("bookreader", "ChapterContent");
            
            Utility.TryActionHelper(delegate()
            {
                // we need 2 round trip to server to get the real file   
                bool isLoading = true;
                bool isSecondLoad = true;

                Browser.DocumentCompleted += (s, e) =>
                {
                    if (isLoading)
                    {
                        Browser.Navigate(realUrl);
                        isLoading = false;
                    }
                    else
                    {
                        if (isSecondLoad)
                        {
                            htmlContent = Browser.DocumentText;
                            isSecondLoad = false;
                        }
                    }
                };
                /*Browser.Navigated += (s, e) =>
                                         {
                                             if (isLoading)
                                             {
                                                 Browser.DocumentText =
                                                 System.Text.RegularExpressions.Regex.Replace(Browser.DocumentText,
                                                    @"<img\s[^>]*>(?:\s*?</img>)?", string.Empty,
                                                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                             }
                                         };*/
                Browser.Navigate(url);
                while (isLoading || isSecondLoad)
                {
                    // do nothing
                }

            }, 1);
            return htmlContent;
        }
    }
}
