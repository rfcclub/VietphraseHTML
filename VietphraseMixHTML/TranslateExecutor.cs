using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Setting = VietphraseMixHTML.Properties.Settings;
using VBStrings = Microsoft.VisualBasic.Strings;
using HtmlAgilityPack;
using Ionic.Zip;
using System.Xml;
using System.Drawing;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace VietphraseMixHTML
{
    public class TranslateExecutor
    {
        public const int BOOK_CHAPTERS = 1000;
        public const int MAX_THREADS = 10;
        IList<string> originalStrings;
        IList<string> links;
        IDictionary<int, string> translateMap;
        IDictionary<int, string> chapterTranslateMap;
        int processCount = 0;
        int startPoint = 0;
        int totalCount;
        bool start;
        FictionObject fictionObject;
        WorkingThread reportThread;
        int mergeLimit;
        bool noMoreSignal = false;
        IList<WorkingThread> workingThreads;
        private static readonly Regex cjkCharRegex = new Regex(@"\p{IsCJKUnifiedIdeographs}");

        private static readonly Regex vnRegex = new Regex(@"[ÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơƯĂẠẢẤẦẨẪẬẮẰẲẴẶẸẺẼỀỀỂẾưăạảấầẩẫậắằẳẵặẹẻẽềềểếỄỆỈỊỌỎỐỒỔỖỘỚỜỞỠỢỤỦỨỪễệỉịọỏốồổỗộớờởỡợụủứừỬỮỰỲỴÝỶỸửữựỳỵỷỹ]+");
        public bool SaveChinese { get; set; }
        public bool SingleFile { get; set; }
        public IList<string> ProcessedLinkList { set; private get; }
        public EventWaitHandle WaitProcess {get; set;}
        private object lockObject = new object();
        public TranslateExecutor(FictionObject fictionObject, int totalCount, int mergeLimit, WorkingThread reportThread)
        {
            this.fictionObject = fictionObject;
            this.reportThread = reportThread;
            this.totalCount = totalCount;
            this.mergeLimit = mergeLimit;
            processCount = 0;
            startPoint = 0;
            originalStrings = new List<string>();
            translateMap = new Dictionary<int, string>();
            chapterTranslateMap = new Dictionary<int, string>();
            workingThreads = new List<WorkingThread>();
        }

        public bool Finish()
        {
            return !start || processCount == totalCount;
        }

        public void Start()
        {
            start = true;
            TranslateChapterNames();
        }

        private void TranslateChapterNames()
        {
            WorkingThread thread = new WorkingThread();
            thread.DoWork += ThreadTranslateName;
            thread.WorkCompleted += Thread_WorkCompleted1;
            workingThreads.Add(thread);
            thread.RunWorkAsync();
        }

        private void Thread_WorkCompleted1(object sender, WorkingEventArg e)
        {
            workingThreads.Remove((WorkingThread)sender);
        }

        private void ThreadTranslateName(object sender, WorkingEventArg e)
        {
            TranslateChaptersTitle();
        }
        public static string StripChineseChars(string input)
        {
            StringBuilder result = new StringBuilder(input);
            result = result.Replace('\uff0c', ',')
                           .Replace('\uff01', '!')
                           .Replace('\uff08', '(')
                           .Replace('\uff09', ')')
                           .Replace('\uff1a', ':')
                           .Replace('\uff1b', ';')
                           .Replace('\uff1f', '?')
                           .Replace('\uff5e', '~')
                           .Replace('\u2026', '.')
                           //.Replace('\u201c', '"')
                           //.Replace('\u201d', '"')
                           .Replace('—', '-')
                           .Replace('鐾', ' ')
                           .Replace('\u203b', '*')
                           .Replace('\u3000', ' ')
                           .Replace('\u3001', ',')
                           .Replace('\u3002', '.')
                           .Replace('\u300a', '(')
                           .Replace('\u300b', ')')
                           .Replace('\u3010', '[')
                           .Replace('\u3011', ']');
            return result.ToString();
        }

        public static StringBuilder StripChineseCharsInSB(StringBuilder result)
        {
            result = result.Replace('\uff0c', ',')
                           .Replace('\uff01', '!')
                           .Replace('\uff08', '(')
                           .Replace('\uff09', ')')
                           .Replace('\uff1a', ':')
                           .Replace('\uff1b', ';')
                           .Replace('\uff1f', '?')
                           .Replace('\uff5e', '~')
                           .Replace('\u2026', '.')
                           //.Replace('\u201c', '"')
                           //.Replace('\u201d', '"')
                           .Replace('—', '-')
                           .Replace('鐾', ' ')
                           .Replace('\u203b', '*')
                           .Replace('\u3000', ' ')
                           .Replace('\u3001', ',')
                           .Replace('\u3002', '.')
                           .Replace('\u300a', '(')
                           .Replace('\u300b', ')')
                           .Replace('\u3010', '[')
                           .Replace('\u3011', ']');
            return result;
        }

        public void TranslateChaptersTitle()
        {
            for (int i = 0; i < fictionObject.NewChapterNamesList.Count; i++)
            {
                string translateContent = DoTranslate(fictionObject.NewChapterNamesList[i]);
                fictionObject.ChapterNamesList.Add(translateContent);
                chapterTranslateMap[i] = translateContent;
                fictionObject.NewChapterNamesList[i] = translateContent;
            }
            if (fictionObject.ChapterNamesList == null) fictionObject.ChapterNamesList = new List<string>();
            fictionObject.ChapterNamesList.AddRange(fictionObject.NewChapterNamesList);
            fictionObject.NewChapterNamesList.Clear();
        }

        private string DoTranslate(string input)
        {
            bool translated = false;
            var orderedKeys = GlobalCache.TranslateMap.Keys.OrderBy(s => s);
            var translatedContent = input;
            foreach (var orderKey in orderedKeys)
            {
                var listKV = GlobalCache.TranslateMap[orderKey].AsEnumerable().ToList();
                int count = 0;
                foreach (var kv in listKV)
                {
                    if (orderKey != GlobalCache.LUATNHAN_ORDER)
                    {
                        translatedContent = VBStrings.Replace(translatedContent, kv.Key, kv.Value + " ");
                    }
                    else
                    {
                        Match match = Regex.Match(translatedContent, kv.Key);
                        if (match.Success)
                        {
                            translatedContent = Regex.Replace(translatedContent, kv.Key, kv.Value);
                        }
                    }
                    count++;
                    if (count % 20 == 0 && !IsChinese(translatedContent))
                    {
                        translated = true;
                        break;
                    }
                }
                if (translated) break;
            }
            GlobalCache.ThanhNgu.AsEnumerable().ToList().ForEach(t =>
                translatedContent = VBStrings.Replace(translatedContent, t.Key, t.Value + " "));
            return translatedContent;
            //GlobalCache.VietPhrase.AsEnumerable().ToList().ForEach(t =>
            //        translateBuilder.Replace(t.Key, t.Value + " "));
            //GlobalCache.Names.AsEnumerable().ToList().ForEach(t =>
            //    translateContent = VBStrings.Replace(translateContent, t.Key, t.Value + " "));
            //GlobalCache.ChinesePhienAmWords.AsEnumerable().ToList().ForEach(t =>
            //    translateContent = VBStrings.Replace(translateContent, t.Key, t.Value + " "));
            //GlobalCache.ThanhNgu.AsEnumerable().ToList().ForEach(t =>
            //    translateContent = VBStrings.Replace(translateContent, t.Key, t.Value + " "));
        }

        public static bool IsChinese(string c)
        {
            return cjkCharRegex.IsMatch(c);
        }

        public static bool IsVietnamese(string c)
        {
            return vnRegex.IsMatch(c);
        }

        public void Stop()
        {
            start = false;
            WaitProcess.Set();
        }

        public void Translate(String content)
        {
            if (start)
            {
                originalStrings.Add(content);
                if (originalStrings.Count == totalCount) noMoreSignal = true;
                StartTranslateThread();
            }
        }

        private bool StartTranslateThread()
        {
            if(workingThreads.Count < MAX_THREADS)
            {
                WorkingThread thread = new WorkingThread();
                thread.DoWork += Thread_DoWork;
                thread.WorkCompleted += Thread_WorkCompleted;
                thread.WorkingProgress += Thread_WorkingProgress;
                thread.Background = true;
                workingThreads.Add(thread);
                thread.RunWorkAsync();
                return true;
            } else
            {
                return false;
            }
            
        }

        private void Thread_WorkingProgress(object sender, WorkingEventArg e)
        {
            
        }

        private void Thread_WorkCompleted(object sender, WorkingEventArg e)
        {            
            workingThreads.Remove((WorkingThread)sender);
            if (workingThreads.Count == 0 && translateMap.Keys.Count == totalCount && start)
            {
                //lock(lockObject)
                //{
                    // SaveTranslateFiles();
                    ConvertToEpubs(false);
                //}
            }
            
        }

        private void ConvertToEpubs(bool overrideAll)
        {
            /*if (fictionObject.ChapterCount <= BOOK_CHAPTERS)
            {
                SaveSingleFile();
            }
            else
            {*/
            SaveEpub();
            // copy template file
            string dir = Setting.Default.Workspace + "\\" + fictionObject.Location;
            PopulateExistFile(dir, overrideAll);
            Stop();
            ConvertAllToKindle(dir, overrideAll);
            //}
        }

        private void SaveSingleFile()
        {
            SaveEpub();
            Stop();
           
        }

        private Image DrawCover(String text, String author, Font font, Font authorFont, Color textColor, Color backColor)
        {
            //first, create a dummy bitmap just to get a graphics object
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            //img = new Bitmap((int)textSize.Width, (int)textSize.Height);
            img = new Bitmap(600, 800);
            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text
            Brush textBrush = new SolidBrush(textColor);

            drawing.DrawString(text, font, textBrush, 50, 50);
            drawing.Save();
            drawing.DrawString(author, authorFont, textBrush, 50, 120);
            textBrush.Dispose();
            drawing.Dispose();

            return img;

        }



        public void SaveMultipleEpub()
        {

            // copy template file
            string dir = Setting.Default.Workspace + "\\" + fictionObject.Location;
            PopulateExistFile(dir, true);
            ConvertAllToKindle(dir, true);
        }

        public void ConvertAllToKindle(string dir, bool overrideAll)
        {
            int steps = fictionObject.ChapterCount;
            int bookCount = steps <= BOOK_CHAPTERS? 1: steps / BOOK_CHAPTERS;
            if (steps > BOOK_CHAPTERS && steps % (bookCount * BOOK_CHAPTERS) > 0) bookCount = bookCount + 1;
            for (int i = 1; i <= bookCount; i++)
            {
                string bookDir = dir + "\\Book_" + i.ToString();
                if (!Directory.Exists(bookDir)) Directory.CreateDirectory(bookDir);
                String bookPath = bookDir + "\\" + Utility.NormalizeName(this.fictionObject.Name) + "_" + i.ToString() + ".mobi";
                bool hasBookExisting = File.Exists(bookPath);
                // if override so delete old version and convert again
                if (hasBookExisting && overrideAll) 
                {
                    File.Delete(bookPath);
                    hasBookExisting = false;
                }
                if (!hasBookExisting || i == bookCount) // no book exists or last piece
                {
                    Image cover = DrawCover(fictionObject.Name + " - Quyển " + i.ToString(), fictionObject.Author,
                    new Font("Arial", 30, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel),
                    new Font("Arial", 18, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel),
                    Color.Black, 
                    Color.White);
                    cover.Save(bookDir + "\\mycover.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                    cover.Dispose();

                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    //startInfo.CreateNoWindow = false;
                    startInfo.UseShellExecute = true;
                    startInfo.WindowStyle = ProcessWindowStyle.Normal;
                    startInfo.FileName = "C:\\kindlegen\\kindlegen.exe";
                    string compressOption = "-c0";
                    /*if (fictionObject.ChapterCount >= 500)
                    {
                        compressOption = "-c0";
                    }*/
                    startInfo.Arguments = bookDir + "\\mykindlebook.opf " + compressOption + " -dont_append_source -o " + Utility.NormalizeName(this.fictionObject.Name) + "_" + i.ToString() + ".mobi";

                    // Start the process with the info we specified.
                    // Call WaitForExit and then the using statement will close.
                    using (Process exeProcess = Process.Start(startInfo))
                    {
                        exeProcess.WaitForExit();
                        
                    }
                }
            }
            ClearKindleFile();
        }

        public void ClearKindleFile()
        {
            // clean books
            string dir = Setting.Default.Workspace + "\\" + fictionObject.Location;
            int steps = fictionObject.ChapterCount;
            int bookCount = steps / BOOK_CHAPTERS;
            if (bookCount == 0) return;
            if (steps % (bookCount * BOOK_CHAPTERS) > 0) bookCount = bookCount + 1;
            for (int i = 1; i <= bookCount; i++)
            {
                string bookDir = dir + "\\Book_" + i.ToString();
                if (!Directory.Exists(bookDir)) continue;
                String bookPath = bookDir + "\\" + Utility.NormalizeName(this.fictionObject.Name) + "_" + i.ToString() + ".mobi";
                // generate smaller mobi file
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = true;
                startInfo.WindowStyle = ProcessWindowStyle.Normal;
                startInfo.FileName = "c:\\kindleunpack\\unpack.cmd";
                startInfo.UseShellExecute = false;
                startInfo.Arguments = bookPath + " " + bookDir;
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }

                string newFile = bookDir + "\\" + "mobi7-" + Utility.NormalizeName(this.fictionObject.Name) + "_" + i.ToString() + ".mobi";
                if (File.Exists(newFile) && File.Exists(bookPath))
                {
                    File.Delete(bookPath);
                    File.Move(newFile, bookPath);
                }

                string[] paths = Directory.GetFiles(bookDir);
                foreach (string path in paths)
                {
                    if (path.Equals(bookPath) || path.EndsWith(".mobi")) continue;
                    File.Delete(path);
                }
                string[] dirs = Directory.GetDirectories(bookDir);
                foreach(string subDir in dirs)
                {
                    DeleteDir(subDir);
                }
            }
        }

        private void DeleteDir(string dir)
        {
            string[] dirs = Directory.GetDirectories(dir);
            foreach (string subDir in dirs) DeleteDir(subDir);
            string[] files = Directory.GetFiles(dir);
            foreach (string file in files) File.Delete(file);
            Directory.Delete(dir);
        }

        private void PopulateExistFile(string dir, bool overrideAll)
        {
            HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.Load(dir + "\\mykindlebook.html");
            HtmlAgilityPack.HtmlDocument tocDocument = new HtmlAgilityPack.HtmlDocument();
            tocDocument.Load(dir + "\\toc.html");
            XmlDocument opfXml = new XmlDocument();
            opfXml.Load(dir + "\\mykindlebook.opf");

            XmlDocument tocXml = new XmlDocument();
            tocXml.Load(dir + "\\toc.ncx");
            var ns = new XmlNamespaceManager(tocXml.NameTable);
            ns.AddNamespace("ns", "http://www.daisy.org/z3986/2005/ncx/");
            XmlNode tocXmlNode = tocXml.SelectSingleNode("/ns:ncx/ns:navMap", ns);

            HtmlNode bodyNode = htmlDocument.DocumentNode.SelectSingleNode("/html/body");
            HtmlNode tocNode = tocDocument.DocumentNode.SelectSingleNode("/html/body/div");

            IDictionary<int, HtmlNode> chapters = new Dictionary<int, HtmlNode>();
            IDictionary<int, HtmlNode> contents = new Dictionary<int, HtmlNode>();
          
            
            int steps = fictionObject.ChapterCount;
            int bookCount = steps <= BOOK_CHAPTERS? 1 : steps / BOOK_CHAPTERS;
            if (steps > BOOK_CHAPTERS && steps % (bookCount * BOOK_CHAPTERS) > 0)
            {
                bookCount = bookCount + 1;
            }
            for (int i = 1;i <=bookCount; i ++)
            {
                string bookDir = dir + "\\Book_" + i.ToString();
                if(!Directory.Exists(bookDir)) Directory.CreateDirectory(bookDir);
                String bookPath = bookDir + "\\" + Utility.NormalizeName(this.fictionObject.Name) + "_" + i.ToString() + ".mobi";
                bool hasBookExisting = File.Exists(bookPath);
                if (hasBookExisting && overrideAll)
                {
                    hasBookExisting = false;
                }
                if (!hasBookExisting || i == bookCount) // if no book exists or last piece
                {
                    File.Copy(GlobalCache.TemplatePath + "mykindlebook.html", bookDir + "\\mykindlebook.html", true);
                    File.Copy(GlobalCache.TemplatePath + "mykindlebook.opf", bookDir + "\\mykindlebook.opf", true);
                    File.Copy(GlobalCache.TemplatePath + "style.css", bookDir + "\\style.css", true);
                    File.Copy(GlobalCache.TemplatePath + "toc.html", bookDir + "\\toc.html", true);
                    File.Copy(GlobalCache.TemplatePath + "toc.ncx", bookDir + "\\toc.ncx", true);
                }
                if (hasBookExisting && i < bookCount) continue;

                // generate content mykindlebook.html and generate content toc.html
                GenerateBookContent(bookDir, i, bodyNode, tocNode, tocXmlNode, bookPath);
            }

        }

        private void GenerateBookContent(string dir, int count, HtmlNode parentBodyNode, HtmlNode parentTocNode, XmlNode parentTocXmlNode, String fullBookName)
        {
            // generate content mykindlebook.html and generate content toc.html
            HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.Load(dir + "\\mykindlebook.html");
            HtmlAgilityPack.HtmlDocument tocDocument = new HtmlAgilityPack.HtmlDocument();
            tocDocument.Load(dir + "\\toc.html");
            XmlDocument tocXml = new XmlDocument();

            tocXml.Load(dir + "\\toc.ncx");
            var ns = new XmlNamespaceManager(tocXml.NameTable);
            ns.AddNamespace("ns", "http://www.daisy.org/z3986/2005/ncx/");

            tocXml.SelectSingleNode("/ns:ncx/ns:docTitle/ns:text", ns).InnerText = fictionObject.Name + "- Quyển " + count.ToString();
            tocXml.SelectSingleNode("/ns:ncx/ns:docAuthor/ns:text", ns).InnerText = fictionObject.Author;
            XmlDocument opfXml = new XmlDocument();
            opfXml.Load(dir + "\\mykindlebook.opf");
            var nsmgr = new XmlNamespaceManager(opfXml.NameTable);
            nsmgr.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
            nsmgr.AddNamespace("ns", "http://www.idpf.org/2007/opf");
            opfXml.SelectSingleNode("/ns:package/ns:metadata/dc:title", nsmgr).InnerText = fictionObject.Name + " - Quyển " + count.ToString();
            opfXml.SelectSingleNode("/ns:package/ns:metadata/dc:creator", nsmgr).InnerText = fictionObject.Author;
            opfXml.SelectSingleNode("/ns:package/ns:metadata/dc:date", nsmgr).InnerText = DateTime.Now.ToString("dd/MM/yyyy");
            opfXml.Save(dir + "\\mykindlebook.opf");

            htmlDocument.DocumentNode.SelectSingleNode("/html/head/title").InnerHtml = fictionObject.Name + " - Quyển " + count.ToString();
            HtmlNode bodyNode = htmlDocument.DocumentNode.SelectSingleNode("/html/body");
            HtmlNode tocNode = tocDocument.DocumentNode.SelectSingleNode("/html/body/div");

            XmlNode tocXmlNode = tocXml.SelectSingleNode("/ns:ncx/ns:navMap", ns);
            int translatedCount = translateMap.Keys.Count;

            int start = (count - 1) * BOOK_CHAPTERS;
            
            int chapterCount = start;
            int max = count * BOOK_CHAPTERS;
            if (max > fictionObject.ChapterCount) max = fictionObject.ChapterCount;
            while (chapterCount < max && chapterCount < parentBodyNode.ChildNodes.Count)
            {
                bodyNode.ChildNodes.Add(parentBodyNode.ChildNodes[chapterCount]);

                // generate toc.html
                tocNode.AppendChild(parentTocNode.ChildNodes[chapterCount]);
                XmlNode oldXmlNode = parentTocXmlNode.ChildNodes[chapterCount];

                // generate content toc.ncx
                XmlNode navPoint = tocXml.CreateNode(XmlNodeType.Element, "navPoint", "http://www.daisy.org/z3986/2005/ncx/");
                XmlNode navLabel = tocXml.CreateNode(XmlNodeType.Element, "navLabel", "http://www.daisy.org/z3986/2005/ncx/");
                XmlNode text = tocXml.CreateNode(XmlNodeType.Element, "text", "http://www.daisy.org/z3986/2005/ncx/");

                text.InnerText = oldXmlNode.ChildNodes[0].ChildNodes[0].InnerText;
                navLabel.AppendChild(text);
                navPoint.AppendChild(navLabel);
                XmlNode content = tocXml.CreateNode(XmlNodeType.Element, "content", "http://www.daisy.org/z3986/2005/ncx/");
                ((XmlElement)content).SetAttribute("src", ((XmlElement)oldXmlNode.ChildNodes[1]).GetAttribute("src"));
                navPoint.AppendChild(content);
                
                ((XmlElement)navPoint).SetAttribute("class", "book");
                ((XmlElement)navPoint).SetAttribute("id", ((XmlElement)oldXmlNode).GetAttribute("id"));
                ((XmlElement)navPoint).SetAttribute("playOrder", ((XmlElement)oldXmlNode).GetAttribute("playOrder"));
                tocXmlNode.AppendChild(navPoint);

                chapterCount++;
            }

            htmlDocument.Save(dir + "\\mykindlebook.html");
            tocDocument.Save(dir + "\\toc.html");
            tocXml.Save(dir + "\\toc.ncx");
        }

        public void SaveEpub()
        {
            // copy template file
            string dir = Setting.Default.Workspace + "\\" + fictionObject.Location;
            Boolean formatFileExists = true;
            if (!File.Exists(dir + "\\mykindlebook.opf"))
            {
                formatFileExists = false;
                File.Copy(GlobalCache.TemplatePath + "mykindlebook.html", dir + "\\mykindlebook.html", true);
                File.Copy(GlobalCache.TemplatePath + "mykindlebook.opf", dir + "\\mykindlebook.opf", true);
                File.Copy(GlobalCache.TemplatePath + "style.css", dir + "\\style.css", true);
                File.Copy(GlobalCache.TemplatePath + "toc.html", dir + "\\toc.html", true);
                File.Copy(GlobalCache.TemplatePath + "toc.ncx", dir + "\\toc.ncx", true);

            }

            // generate content mykindlebook.html and generate content toc.html
            HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.Load(dir + "\\mykindlebook.html");
            HtmlAgilityPack.HtmlDocument tocDocument = new HtmlAgilityPack.HtmlDocument();
            tocDocument.Load(dir + "\\toc.html");

            XmlDocument tocXml = new XmlDocument();

            tocXml.Load(dir + "\\toc.ncx");
            var ns = new XmlNamespaceManager(tocXml.NameTable);
            ns.AddNamespace("ns", "http://www.daisy.org/z3986/2005/ncx/");

            tocXml.SelectSingleNode("/ns:ncx/ns:docTitle/ns:text", ns).InnerText = fictionObject.Name;
            tocXml.SelectSingleNode("/ns:ncx/ns:docAuthor/ns:text", ns).InnerText = fictionObject.Author;
            XmlDocument opfXml = new XmlDocument();
            opfXml.Load(dir + "\\mykindlebook.opf");
            var nsmgr = new XmlNamespaceManager(opfXml.NameTable);
            nsmgr.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
            nsmgr.AddNamespace("ns", "http://www.idpf.org/2007/opf");
            opfXml.SelectSingleNode("/ns:package/ns:metadata/dc:title", nsmgr).InnerText = fictionObject.Name;
            opfXml.SelectSingleNode("/ns:package/ns:metadata/dc:creator", nsmgr).InnerText = fictionObject.Author;
            opfXml.SelectSingleNode("/ns:package/ns:metadata/dc:date", nsmgr).InnerText = DateTime.Now.ToString("dd/MM/yyyy");
            opfXml.Save(dir + "\\mykindlebook.opf");
            
            var links = htmlDocument.DocumentNode.SelectNodes("//a");
            int step = 0;
            if(links != null && links.Count > 0)
            {
                step = links.Count;
            }


            htmlDocument.DocumentNode.SelectSingleNode("/html/head/title").InnerHtml = fictionObject.Name;
            HtmlNode bodyNode = htmlDocument.DocumentNode.SelectSingleNode("/html/body");
            HtmlNode tocNode = tocDocument.DocumentNode.SelectSingleNode("/html/body/div");

            XmlNode tocXmlNode = tocXml.SelectSingleNode("/ns:ncx/ns:navMap", ns);
            int translatedCount = translateMap.Keys.Count;
            for( int i = 0; i < translatedCount; i++ )
            {
                step++;
                HtmlNode htmlNode = HtmlNode.CreateNode("<div><a name=\"chapter" + step + "\"><h1>" + step + ": " + chapterTranslateMap[i] + "</h1></a></div>");
                string formattedContent = translateMap[i]!=null ? translateMap[i].Replace("\r\n", "<br />") : "NO_CONTENT_HERE";
               
                htmlNode.AppendChild(HtmlNode.CreateNode("<br />"));
                HtmlNode contentNode = HtmlNode.CreateNode("<span>temp</span");
                contentNode.InnerHtml = formattedContent;
                htmlNode.AppendChild(contentNode);
                bodyNode.AppendChild(htmlNode);
                
                // generate toc.html
                tocNode.AppendChild(HtmlNode.CreateNode("<div><a href=\"mykindlebook.html#chapter" + step + "\">" + step + ": " + chapterTranslateMap[i]+ " </a></div>"));

                // generate content toc.ncx
                XmlNode navPoint = tocXml.CreateNode(XmlNodeType.Element, "navPoint", "http://www.daisy.org/z3986/2005/ncx/");
                XmlNode navLabel = tocXml.CreateNode(XmlNodeType.Element, "navLabel", "http://www.daisy.org/z3986/2005/ncx/");
                XmlNode text = tocXml.CreateNode(XmlNodeType.Element, "text", "http://www.daisy.org/z3986/2005/ncx/");
                text.InnerText = chapterTranslateMap[i];
                navLabel.AppendChild(text);
                navPoint.AppendChild(navLabel);
                XmlNode content = tocXml.CreateNode(XmlNodeType.Element, "content", "http://www.daisy.org/z3986/2005/ncx/");
                ((XmlElement)content).SetAttribute("src", "mykindlebook.html#chapter" + step);
                navPoint.AppendChild(content);
                // navPoint.InnerXml = "<navLabel><text>" + chapterTranslateMap[i] + "</text></navLabel><content src=\"mykindlebook.html#chapter" + step + "\"/>";
                ((XmlElement)navPoint).SetAttribute("class", "book");
                ((XmlElement)navPoint).SetAttribute("id", "level" + step);
                ((XmlElement)navPoint).SetAttribute("playOrder", (step + 1).ToString());
                tocXmlNode.AppendChild(navPoint);
                // generate content mykindlebook.opf

            }
            htmlDocument.Save(dir + "\\mykindlebook.html");
            tocDocument.Save(dir + "\\toc.html");
            tocXml.Save(dir + "\\toc.ncx");

            fictionObject.FilesList.AddRange(ProcessedLinkList);
            ProcessedLinkList.Clear();
        }

        public void SaveTranslateFiles()
        {
            //TODO
            int translatedCount = translateMap.Keys.Count;            
            while (startPoint < translatedCount)
            {
                int stepCount = fictionObject.GetPreviousStepCount();
                stepCount += (int)(translatedCount / mergeLimit) + 1;
                StringBuilder contentBuilder = new StringBuilder();
                int nextStop = startPoint + mergeLimit;
                // if next stop larger than translated limit so nextstop is that limit
                if (nextStop > translatedCount) nextStop = translatedCount;

                for (int i = startPoint; i < nextStop; i++)
                {
                    contentBuilder.Append(chapterTranslateMap[i] + "\r\n");
                    contentBuilder.Append(translateMap[i] + "\r\n");
                }
                SaveToDisk(contentBuilder.ToString(), stepCount);
                if (SaveChinese)
                {
                    StringBuilder originalBuilder = new StringBuilder();
                    for (int i = startPoint; i < nextStop; i++)
                    {
                        originalBuilder.Append(originalStrings[i]);
                    }
                    SaveChineseToDisk(originalBuilder.ToString(), stepCount);
                }
                startPoint = nextStop;                
            }
            fictionObject.FilesList.AddRange(ProcessedLinkList);
            ProcessedLinkList.Clear();
            if (SingleFile)
            {
                string dir = Setting.Default.Workspace + "\\" + fictionObject.Location;
                String zipFile = dir + @"\" + Utility.NormalizeName(fictionObject.Name) + ".zip";
                String txtFile = dir + @"\" + Utility.NormalizeName(fictionObject.Name) + ".txt";
                int step = fictionObject.GetPreviousStepCount() < 1 ? 1 : fictionObject.GetPreviousStepCount();
                string path = dir + "\\" + string.Format("{0:000}", step) + ".txt";
                if (File.Exists(path))
                {
                    if (File.Exists(zipFile))
                    {
                        File.Delete(zipFile);
                    }
                    using (ZipFile zip = new ZipFile())
                    {
                        zip.AddFile(path, "");
                        zip.Save(zipFile);
                    }

                    File.Copy(path, txtFile, true);
                    File.Delete(path);
                }
            }
            Stop();
        }
        
        private void Thread_DoWork(object sender, WorkingEventArg e)
        {
            while (processCount < originalStrings.Count && processCount < totalCount)
            {
                int count = GetNextCount();
                string original = originalStrings[count];

                string result = original;

                if (!original.StartsWith("VPBOBBIE"))
                {
                    original = Utility.CleanContent(original, fictionObject);
                    result = Translate(original, reportThread);
                }
                else
                    result = original.Substring(8);

                translateMap[count] = result;
                reportThread.ReportProgress(count);
            }
        }

        private int GetNextCount()
        {
            int count = -1;
            lock (lockObject)
            {
                count = processCount;
                processCount += 1;
            }
            return count;
        }

        private string Translate(string originalContent, object sender)
        {
            string translateContent = originalContent;
            StringBuilder content = new StringBuilder(translateContent);
            string status = "VANBAN:" + content.Length.ToString() + " characters -- ";
            long curTick = DateTime.Now.Ticks;
            translateContent = DoTranslate(originalContent);
            //GlobalCache.VietPhrase.AsEnumerable().ToList().ForEach(t =>
            //    translateContent = VBStrings.Replace(translateContent, t.Key, t.Value + " "));
            //GlobalCache.Names.AsEnumerable().ToList().ForEach(t =>
            //    translateContent = VBStrings.Replace(translateContent, t.Key, t.Value + " "));
            //GlobalCache.ChinesePhienAmWords.AsEnumerable().ToList().ForEach(t =>
            //    translateContent = VBStrings.Replace(translateContent, t.Key, t.Value + " "));
            //GlobalCache.ThanhNgu.AsEnumerable().ToList().ForEach(t =>
            //    translateContent = VBStrings.Replace(translateContent, t.Key, t.Value + " "));

            long endTick = DateTime.Now.Ticks - curTick;
            ((WorkingThread)sender).ReportProgress(0, status + "TRANSLATED:" + (endTick / 10000000).ToString() + "s");
            return translateContent;
        }

        private void SaveToDisk(string content, int stepCount)
        {
            if (fictionObject.HTMLLink.IndexOf("zongheng") >= 0)
            {
                content = Utility.CleanZongHeng(content);
            }

            string dir = Setting.Default.Workspace + "\\" + fictionObject.Location;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (SingleFile)
            {
                int step = fictionObject.PreviousStepCount;
                if (step != 1) step = 1;
                string path = dir + "\\" + string.Format("{0:000}", step) + ".txt";
                if (File.Exists(path))
                {
                    File.AppendAllText(path, content, Encoding.UTF8);
                }
                else
                {
                    File.WriteAllText(path, content, Encoding.UTF8);
                }
            }
            else
            {
                string path = dir + "\\" + string.Format("{0:000}", stepCount) + ".txt";
                File.WriteAllText(path, content, Encoding.UTF8);
            }
            //File.Create(path);

        }
        private void SaveChineseToDisk(string content, int stepCount)
        {
            string dir = Setting.Default.Workspace + "\\" + fictionObject.Location + @"\\Chinese";            
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string path = dir + "\\" + string.Format("{0:000}", stepCount) + ".txt";
            //File.Create(path);
            File.WriteAllText(path, content, Encoding.UTF8);
        }
    }
}
