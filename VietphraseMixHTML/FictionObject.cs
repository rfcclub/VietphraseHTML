using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Setting = VietphraseMixHTML.Properties.Settings;

namespace VietphraseMixHTML
{
    [Serializable]
    public class FictionObject
    {
        public string Author { get; set; }

        public bool ResetFlag {get; set;}

        public string UpdateSignString { get; set;}

        public UpdateSignType UpdateSign { get; set; }
        public int PreviousStepCount { get; set; }
        public string Name { get; set; }
        public string HTMLLink { get; set; }
        public string Location { get; set; }
        public string DownloaderName { get; set; }
        public List<string> FilesList { get; set; }
        public List<string> NewFilesList { get; set; }
        public List<string> ChapterNamesList { get; set; }
        public List<string> NewChapterNamesList { get; set; }

        public void UpdateContentList()
        {
            if (NewFilesList != null && NewFilesList.Count > 0)
            {
                IList<string> list = new List<string>();
                for (int i=0;i < NewFilesList.Count; i++)
                {
                    list.Add(NewFilesList[i]);

                }
                if(ContentList == null) ContentList = new Dictionary<string, IList<string>>();
                ContentList[DateTime.Now.ToString("ddMMyyyyHHmmss")] = list;
            }
        }

        public Queue<string> ProcessQueue
        {
            get;
            set;
        }
        public bool UseGoogleMobile { get; set; }
        public bool UseVpBotbie { get; set; }
        
        
        public Encoding ContentEncoding { get; set; }
        public bool SortBeforeDownload { get; set; }
        public IDictionary<string, IList<string>> ContentList { get; set; }
        public int ChapterCount
        {
            get
            {
                return FilesList.Count;
            }
        }

        public  int NewChapterCount
        {
            get
            {
                return NewFilesList.Count;
            }
        }

        public FictionObject()
        {
            Iniatilize();
        }

        private void Iniatilize()
        {
            FilesList = new List<string>();
            NewFilesList = new List<string>();
            ProcessQueue = new Queue<string>();
            UpdateSign = UpdateSignType.None;
            ContentList = new Dictionary<string, IList<string>>();
            ContentEncoding = Encoding.GetEncoding("GB2312");
            ChapterNamesList = new List<string>();
            NewChapterNamesList = new List<string>();
        }

        public override bool Equals(object obj)
        {
            if (obj!= null && obj.GetType() == typeof(FictionObject))
            {
                if(!string.IsNullOrEmpty(Name))
                {
                    return Name.Equals(((FictionObject) obj).Name);
                }

                if (!string.IsNullOrEmpty(Location))
                {
                    return Location.Equals((obj as FictionObject).Location);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public void Clear()
        {
            FilesList.Clear();
            NewFilesList.Clear();
            ProcessQueue.Clear();
        }

        public void Save()
        {           
           string location = Setting.Default.Workspace + "\\" + Location;
           if (!Directory.Exists(location))
           {
               Directory.CreateDirectory(location);
           }

            Stream mstStream = null;
            //string fileName = Location + "\\" + Name + ".fobj2";
            string fileName = location + "\\" + Name + ".fobj3";
            string fileName1 = location + "\\" + Name + ".fobj";
            if (ContentEncoding != Encoding.UTF8 && ContentEncoding != Encoding.GetEncoding("GB2312"))
            {
                ContentEncoding = Encoding.GetEncoding("GB2312");
            }
            try
            {
                if (File.Exists(fileName))
                {
                    File.Copy(fileName, fileName + ".bkp", true);
                    File.Delete(fileName);
                }

                StreamWriter writer = new StreamWriter(fileName,
                                      false,
                                      Encoding.UTF8);
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, this);
                writer.Flush();
                writer.Close();
                if (File.Exists(fileName1))
                {
                    File.Copy(fileName1, fileName1 + ".bkp", true);
                    File.Delete(fileName1);
                }
                mstStream = File.Open(fileName1, FileMode.Create);
                //var writer = new TypeSerializer<FictionObject>();
                //var streamWriter = new StreamWriter(mstStream);
                //writer.SerializeToWriter(this, streamWriter);
                //streamWriter.Flush();
                BinaryFormatter mstBf = new BinaryFormatter();
                mstBf.Serialize(mstStream, this);
                mstStream.Flush();

            }
            finally
            {
                if(mstStream!= null)
                {
                    mstStream.Close();
                }
            }
        }

        public void Update()
        {
            if(UpdateSign == UpdateSignType.None)
            {
                return;
            }
            Utility.TryActionHelper(DoUpdate,3);
            UpdateContentList();
        }

        private void DoUpdate()
        {
            NewFilesList.Clear();
            NewChapterNamesList.Clear();
            List<string> lstLinks = new List<string>();
            HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
            WebRequest request = WebRequest.Create(HTMLLink);
            htmlDocument.Load(request.GetResponse().GetResponseStream(), ContentEncoding);
            string buildNewLink = null;
            bool moveToRoot = false;
            HtmlNodeCollection links = htmlDocument.DocumentNode.SelectNodes("//a");
            foreach (HtmlNode link in links)
            {
                HtmlAttribute att = link.Attributes["href"];

                if (att == null) continue;
                string newLink = att.Value;
                if (!MatchUpdateSign(newLink)) continue;
                if (!newLink.StartsWith("http://"))
                {
                    if (newLink.StartsWith("/"))
                    {
                        newLink = newLink.Remove(0, 1);
                        moveToRoot = true;
                    }
                    if (newLink.StartsWith("wenxue"))
                    {
                        newLink = newLink.Substring(newLink.LastIndexOf("/") + 1);
                    }

                    if (!moveToRoot)
                    {
                        buildNewLink = HTMLLink.Substring(0, HTMLLink.LastIndexOf("/") + 1) + newLink;
                    }
                    else
                    {
                        int startPoint = HTMLLink.StartsWith("https") ? 8 : 7;
                        buildNewLink = HTMLLink.Substring(0, HTMLLink.IndexOf("/", startPoint) + 1) + newLink;
                        moveToRoot = false;
                    }
                }
                else
                {
                    buildNewLink = newLink;    
                }
                
                if (!FilesList.Contains(buildNewLink))
                {
                    NewFilesList.Add(buildNewLink);
                    NewChapterNamesList.Add(GetUTF8Content(HTMLLink, link.InnerHtml, ContentEncoding));
                }
            }

            if(SortBeforeDownload)
            {
                NewFilesList = DoSort(NewFilesList);
            }
            PreviousStepCount = GetPreviousStepCount();

            
        }

        private string GetUTF8Content(string url, string htmlContent, Encoding chineseEncoding)
        {
            string utf8Content = null;
            if (!string.IsNullOrEmpty(htmlContent))
            {
                if (Utility.IsUtf8Site(url))
                {
                    utf8Content = htmlContent;
                }
                else
                {
                    utf8Content = chineseEncoding.GetString(chineseEncoding.GetBytes(htmlContent));
                }
            }
            else
            {
                utf8Content = "Page not found!";
            }
            return utf8Content;
        }

        private List<string> DoSort(List<string> lstLinks)
        {
             List<string>_sortUrlList = new List<string>();
            IEnumerable<string> source = lstLinks.OfType<string>();
            var sorted = (from item in source
                          orderby item.Length, item ascending
                          select item).ToList();
            
            foreach (string itms in sorted)
            {
                _sortUrlList.Add(itms);
            }
            return _sortUrlList;
        }

        public int GetPreviousStepCount()
        {
            int previousStepCount = 0;
            string[] files = Directory.GetFiles(Setting.Default.Workspace + "\\" + Location);
            foreach (string file in files)
            {
                if(!file.EndsWith("txt")) continue;
                int test = 0;
                int startIndex = file.LastIndexOf("\\") + 1;
                int endIndex = file.LastIndexOf(".") - startIndex;
                Utility.TryActionHelper((delegate() { test = int.Parse(file.Substring(startIndex,endIndex )); }), 1);

                if(test > previousStepCount) previousStepCount = test;
            }
            return previousStepCount;
        }

        private bool MatchUpdateSign(string link)
        {
            switch (UpdateSign)
            {
                case UpdateSignType.Incremental:
                    long test = 0;
                    Utility.TryActionHelper(delegate() { test = long.Parse(link.Substring(0, link.IndexOf("."))); }, 1);
                    if(test > 0) return true;
                    break;
                case UpdateSignType.StringPrefix:
                    return link.StartsWith(UpdateSignString) && !link.EndsWith(UpdateSignString) ? true : false;
                case UpdateSignType.RegExp:
                    return System.Text.RegularExpressions.Regex.IsMatch(link, UpdateSignString);
                default:
                    return false;
            }
            return false;
        }

        public void RecalculatePreviousStepCount()
        {
            PreviousStepCount = GetPreviousStepCount();
        }

        public void Reset()
        {
            Clear();
            Iniatilize();
            // turn on reset flag
            ResetFlag = true;
        }
    }

    [Serializable]
    public enum UpdateSignType { None,Incremental, StringPrefix, RegExp }
}
