using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using HtmlAgilityPack;
using InfoBox;
using VietphraseMixHTML.Downloaders;
using Setting = VietphraseMixHTML.Properties.Settings;
using VBStrings = Microsoft.VisualBasic.Strings;
using Ionic.Zip;

namespace VietphraseMixHTML
{
    public partial class MainForm : Form
    {
        private const int VIEW_MODE = 0;
        private const int EDIT_MODE = 1;
        private const int ADD_MODE = 2;
        private const int START_JOB_MODE = 3;
        private int _currentMode = 0;

        private FictionObject _currentFictionObject = null;
        private int _maxProcessingCount = 0;
        private int _currentProcessingCount = 0;
        private int _currentBookCount = 0;
        private int _stepCount = 1;
        
        private StringBuilder _contentBuilder = null;
        private StringBuilder _originalBuilder = null;
        private StringBuilder _threeTypeBuilder = null;
        
        private Queue<string> _processingQueue = new Queue<string>();
        private FictionObjectManager _fictionObjectManager;
        private List<string> _processedLinkList = new List<string>();
        private string QIDIAN_SITE = "qidian.com/BookReader";

        private WorkingThread downloadThread = null;
        private WorkingThread translateThread = null;
        private ProcessingQueue<string> processingQueue = null;
        private BackgroundWorker translateWorker = null;
        protected EventWaitHandle waitProcess;
        private Queue<int> grandProcessQueue = new Queue<int>();

        private Encoding _pageEncoding = null;
        
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the btnStart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            // check error on GUI before starting
            if (!CheckIntegrity()) return;

            // save current fictional object
            BtnSaveFictionObjectClick(null,null);
            ProcessCurrentFictionObject();
           
        }

        private void ProcessCurrentFictionObject()
        {
            LockInformation(START_JOB_MODE);
            RecalculateStepCount();
            
            // if it does not have file list 
            if (_currentFictionObject.FilesList.Count == 0)
            { // open form for download new book
                GetLinksForm form = new GetLinksForm();
                form.Url = _currentFictionObject.HTMLLink;
                form.UpdateSignString = _currentFictionObject.UpdateSignString;
                form.UpdateSign = _currentFictionObject.UpdateSign;
                form.ShowDialog();

                _currentFictionObject.NewFilesList = form.UrlList;
                _currentFictionObject.NewChapterNamesList = form.ChapterNameList;
                _currentFictionObject.UpdateContentList();
                _currentFictionObject.UpdateSignString = form.UpdateSignString;
                _currentFictionObject.UpdateSign = form.UpdateSign;
                _currentFictionObject.SortBeforeDownload = form.SortBeforeDownload;
                _pageEncoding = form.PageEncoding;
                _stepCount = 1;
                _currentFictionObject.Save();
            }


            // if it does not have link for download then check the fiction object list to get other fiction object to download
            if (_currentFictionObject.NewFilesList == null
                || _currentFictionObject.NewFilesList.Count == 0)
            {
                // check the fiction object list to get other fiction object to download
                CheckGrandQueue();
            }
            else // process current fiction object
            {
                lblTotalFile.Text = _currentFictionObject.NewFilesList.Count.ToString();
                _maxProcessingCount = _currentFictionObject.NewFilesList.Count;
                progressDownloadBar.Maximum = _maxProcessingCount;
                progressTranslateBar.Maximum = _maxProcessingCount;
                _currentProcessingCount = 0;
                _currentBookCount = 0;

                waitProcess = new EventWaitHandle(false, EventResetMode.AutoReset);

                downloadThread = new WorkingThread();
                downloadThread.Background = chkRunBackground.Checked;
                downloadThread.DoWork += new EventHandler<WorkingEventArg>(downloadThread_DoWork);
                downloadThread.WorkingProgress += new EventHandler<WorkingEventArg>(downloadThread_WorkingProgress);


                translateThread = new WorkingThread();
                translateThread.Background = chkRunBackground.Checked;
                translateThread.DoWork += new EventHandler<WorkingEventArg>(translateWorker_DoWork);
                translateThread.WorkingProgress += new EventHandler<WorkingEventArg>(translateWorker_WorkingProgress);
                translateThread.WorkCompleted += new EventHandler<WorkingEventArg>(translateWorker_WorkCompleted);
                
                if (chkSingleFile.Checked)
                {
                    string dir = txtLocation.Text.Trim();
                    String zipFile = dir + @"\" + _currentFictionObject.Name + ".zip";

                    if(File.Exists(zipFile))
                    {
                        using (ZipFile zip = ZipFile.Read(zipFile))
                        {
                            foreach(ZipEntry entry in zip)
                            {
                                entry.Extract(dir, ExtractExistingFileAction.OverwriteSilently);
                            } 
                        }
                    }
                }
                downloadThread.RunWorkAsync();
                translateThread.RunWorkAsync();

            }

        }

        private void RecalculateStepCount()
        {
            // recalculate the next file will be named
            _stepCount = 1;
            _currentFictionObject.RecalculatePreviousStepCount();
            if (_currentFictionObject.PreviousStepCount > 0)
            {
                // _stepCount will use for name the next downloading file
                _stepCount = _currentFictionObject.PreviousStepCount + 1;
            }
        }

        /// <summary>
        /// Handles the RunWorkerCompleted event of the translateWorker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
        void translateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DoCompleteWorks();
        }

        /// <summary>
        /// Does the complete works.
        /// </summary>
        private void DoCompleteWorks()
        {
            //FictionObjectManager.Instance.Save();
            
            _currentFictionObject.Save();
            _currentFictionObject.PreviousStepCount = _stepCount;
            ResetPrivateVariables();
            LockInformation(VIEW_MODE);
            StopAllThreads();
            InformationBox.Show("OK!", new AutoCloseParameters(1));
            RefreshDataGrid();
            _currentFictionObject = null;
            CheckGrandQueue();
        }

        /// <summary>
        /// Resets the private variables.
        /// </summary>
        private void ResetPrivateVariables()
        {
            _currentProcessingCount = 0;
            _currentBookCount = 0;
            _maxProcessingCount = 0;
            progressTranslateBar.Value = 0;
            progressDownloadBar.Value = 0;
        }

        /// <summary>
        /// Stops all threads.
        /// </summary>
        private void StopAllThreads()
        {
            if (translateThread != null)
            {
                translateThread.Stop();
                translateThread = null;
            }
            if (downloadThread != null)
            {
                downloadThread.Stop();
                downloadThread = null;
            }
            
        }


        /// <summary>
        /// Translates the worker_ work completed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void translateWorker_WorkCompleted(object sender, WorkingEventArg e)
        {
            DoCompleteWorks();
        }

        /// <summary>
        /// Checks the grand queue.
        /// </summary>
        private void CheckGrandQueue()
        {
            if (grandProcessQueue.Count > 0)
            {
                int index = grandProcessQueue.Dequeue();
                if (index >= dgvFictions.Rows.Count) index = dgvFictions.Rows.Count - 1;
                dgvFictions.CurrentCell = dgvFictions[1, index];
                _currentFictionObject = FictionObjectManager.Instance.ProjectList[index];
                LoadFictionObjectToForm();
                btnStart_Click(null, null);

                //if(translateThread == null)
                //{
                    //CheckGrandQueue();
                //}
            }
        }


        /// <summary>
        /// Translates the worker_ working progress.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void translateWorker_WorkingProgress(object sender, WorkingEventArg e)
        {
            if (e.UserState != null)
            {
                translateLabel.Text = (string)e.UserState;
                return;
            }
            if(e.ProgressPercentage <=progressTranslateBar.Maximum)
                progressTranslateBar.Value = e.ProgressPercentage;
            else
                progressTranslateBar.Value = progressTranslateBar.Maximum;
            lblTranslateFile.Text = e.ProgressPercentage.ToString();
            dgvFictions.Refresh(); 
        }

        /// <summary>
        /// Translates the worker_ do work.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void translateWorker_DoWork(object sender, WorkingEventArg e)
        {
            FictionTranslate(sender);
        }

        /// <summary>
        /// Downloads the thread_ working progress.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void downloadThread_WorkingProgress(object sender, WorkingEventArg e)
        {
            lblDownloadFile.Text = e.ProgressPercentage.ToString();
            progressDownloadBar.Value = e.ProgressPercentage;
            dgvFictions.Refresh(); 
        }

        /// <summary>
        /// Downloads the thread_ do work.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void downloadThread_DoWork(object sender, WorkingEventArg e)
        {
            FictionDownload(sender);
        }



        /// <summary>
        /// Checks the integrity.
        /// </summary>
        /// <returns></returns>
        private bool CheckIntegrity()
        {
            if(string.IsNullOrEmpty(txtName.Text))
            {
                return false;
            }
            if (string.IsNullOrEmpty(txtHTMLLink.Text))
            {
                return false;
            }
            if(string.IsNullOrEmpty(txtLocation.Text))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Fictions the download.
        /// </summary>
        /// <param name="sender">The sender.</param>
        private void FictionDownload(object sender)
        {
            bool pageNotFound = false;
            
            //WebClient webClient = new WebClient();
            WebClient webClient = PrepareWebClient();
            //Encoding chineseEncoding = Encoding.GetEncoding("GB2312");
            Encoding chineseEncoding =webClient.Encoding;
            Encoding utf8Encode = Encoding.UTF8;
                        
            long mergeLimit = Int32.Parse(txtLimit.Text);
            int count = 0;
            StringBuilder builder = null;
            int stepCount = 1;
            // if site is Unicode site then set encoding again
            if (Utility.IsUtf8Site(_currentFictionObject.HTMLLink))
            {
                webClient.Encoding = utf8Encode;
            }

            if (_currentFictionObject.ContentEncoding == null)
            {   
                _currentFictionObject.ContentEncoding = webClient.Encoding;
                _currentFictionObject.Save();
            }
            // if we use vpbobbie so set encoding again and set a boolean define content is UTF-8 already
            bool isUTF8Already = false;
            if (chkUseVPBot.Checked)
            {
                webClient.Encoding = utf8Encode;
                isUTF8Already = true;
            }
            // get downloader
            IDownloader downloader = CreateDownloader(_currentFictionObject);
            downloader.Browser = browser;
            downloader.Client = webClient;
                
            for(int i=0;i<_currentFictionObject.NewChapterNamesList.Count; i++)
            {
                string titleContent = _currentFictionObject.NewChapterNamesList[i];
                if(!isUTF8Already)
                {
                    titleContent = GetUTF8Content(_currentFictionObject.HTMLLink, titleContent, chineseEncoding, out pageNotFound);
                }
                _currentFictionObject.NewChapterNamesList[i] = titleContent;
            }
            int currentCount = 0;
            foreach (var url in _currentFictionObject.NewFilesList)
            {
                bool isExisting = false;
                foreach(var curUrl in _currentFictionObject.FilesList)
                {
                    if (curUrl.Equals(url.Trim()))
                    {
                        isExisting = true;
                        break;
                    }
                }

                if (isExisting) continue;

                string htmlContent = null;
                
                // process content and convert it from chinese encoding to UTF-8 encoding
                //if (!chkNativeWebClient.Checked)
                //{
                pageNotFound = false;
                if (!String.IsNullOrEmpty(url.Trim()))
                {
                    // use downloader to download truyen. :D :D :D
                    htmlContent = downloader.Download(url);
                    //string content = 
                    string utf8Content = null;

                    if (isUTF8Already) // if we use bot for getting truyen so it doesnt need to convert again
                        utf8Content = htmlContent;
                    else   // convert truyen to UTF-8 
                        utf8Content = GetUTF8Content(url, htmlContent, chineseEncoding, out pageNotFound);

                    if (!pageNotFound) // gotten page content 
                    {
                        if (!_processedLinkList.Contains(url))
                        {
                            _processedLinkList.Add(url);
                        }
                    }

                    // translate chapter name
                    
                    _processingQueue.Enqueue(utf8Content);
                    waitProcess.Set(); // inform for waiting translate thread
                }
                currentCount++;
                ((WorkingThread)sender).ReportProgress(currentCount);
                //}
            }
            _currentFictionObject.NewFilesList.Clear();
        }

        /// <summary>
        /// Creates the downloader.
        /// </summary>
        /// <param name="currentFictionObject">The current fiction object.</param>
        /// <returns>the downloader</returns>
        private IDownloader CreateDownloader(FictionObject currentFictionObject)
        {
            string downloaderName = DetermineDownloaderName(currentFictionObject);
            Type type = Type.GetType(GlobalCache.Downloaders[downloaderName]);
            return (IDownloader)Activator.CreateInstance(type);
        }

        /// <summary>
        /// Determines the name of the downloader.
        /// </summary>
        /// <param name="currentFictionObject">The current fiction object.</param>
        /// <returns></returns>
        private string DetermineDownloaderName(FictionObject currentFictionObject)
        {
            string downloaderName = currentFictionObject.DownloaderName;
            if (string.IsNullOrEmpty(downloaderName))
            { // create downloader name base on the site name
                downloaderName = DownloaderCollection.DefaultDownloader;
                if (chkGoogleTranslate.Checked)
                    return DownloaderCollection.GoogleTranslateDownloader;
                if (chkUseVPBot.Checked)
                    return DownloaderCollection.VPBobbieDownloader;

                string url = currentFictionObject.HTMLLink;
                foreach (KeyValuePair<string, string> downloaderSignature in GlobalCache.DownloaderSignatures)
                {
                    if(url.IndexOf(downloaderSignature.Value) >= 0)
                    {
                        downloaderName = downloaderSignature.Key;
                        break;
                    } 
                }
            }
            return downloaderName;
        }

        /// <summary>
        /// Prepares the web client.
        /// </summary>
        /// <returns></returns>
        private WebClient PrepareWebClient()
        {
            WebClient webClient = new WebClient();
            Encoding chineseEncoding = null;
            webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            if (_currentFictionObject.ContentEncoding == null)
            {
                // try to get encoding from web site
                byte[] data = webClient.DownloadData(_currentFictionObject.HTMLLink);
                string pageEncodingString = webClient.ResponseHeaders["Content-Encoding"];
                // check whether we can get a valid encoding ??
                if (string.IsNullOrEmpty(pageEncodingString))
                {
                    pageEncodingString = webClient.ResponseHeaders["Content-Type"];
                    if (!string.IsNullOrEmpty(pageEncodingString)
                         && pageEncodingString.IndexOf("charset=", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        pageEncodingString =
                            pageEncodingString.Substring(
                                pageEncodingString.IndexOf("charset=", StringComparison.OrdinalIgnoreCase) + 8);
                    }
                    else
                    {
                        pageEncodingString = null;
                    }
                }

                if (!string.IsNullOrEmpty(pageEncodingString)) _pageEncoding = Encoding.GetEncoding(pageEncodingString);
                if (_pageEncoding != null) // set founded encoding to webclient
                    chineseEncoding = _pageEncoding;
                else // default Encoding is GB2312
                    chineseEncoding = Encoding.GetEncoding("GB2312");
                webClient.Encoding = chineseEncoding;
            }
            else
            {
                webClient.Encoding = _currentFictionObject.ContentEncoding;
            }

            return webClient;
        }

        /// <summary>
        /// Determines whether [is hide TXT site] [the specified URL].
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>
        ///   <c>true</c> if [is hide TXT site] [the specified URL]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsHideTxtSite(string url)
        {
            if (url.IndexOf(QIDIAN_SITE) > 0) return true;
            if (url.IndexOf("www.365zw.com") > 0) return true;
            if (url.IndexOf("www.kenwen.com") > 0) return true;
            if (url.IndexOf("www.yuanchuang.com") > 0) return true;
            return false;
        }

        /// <summary>
        /// Gets the content of the UT f8.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="htmlContent">Content of the HTML.</param>
        /// <param name="chineseEncoding">The chinese encoding.</param>
        /// <param name="pageNotFound">if set to <c>true</c> [page not found].</param>
        /// <returns></returns>
        private string GetUTF8Content(string url, string htmlContent, Encoding chineseEncoding, out bool pageNotFound)
        {
            string utf8Content = null;
            if (!string.IsNullOrEmpty(htmlContent))
            {
                pageNotFound = false;
                if (IsUTF8Site(GlobalCache.UTF8Sites, url))
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
                pageNotFound = true;
                utf8Content = "Page not found!";
            }
            return utf8Content;
        }

        /// <summary>
        /// Determines whether [is UT f8 site] [the specified UTF8 sites].
        /// </summary>
        /// <param name="utf8Sites">The UTF8 sites.</param>
        /// <param name="url">The URL.</param>
        /// <returns>
        ///   <c>true</c> if [is UT f8 site] [the specified UTF8 sites]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsUTF8Site(IList<string> utf8Sites, string url)
        {
            foreach (string utf8Site in utf8Sites)
            {
                if (url.IndexOf(utf8Site) >= 0) return true;
            }
            return false;
        }

        /// <summary>
        /// Fictions the translate.
        /// </summary>
        /// <param name="sender">The sender.</param>
        private void FictionTranslate(object sender)
        {

            int mergeLimit = Int32.Parse(txtLimit.Text);
            TranslateExecutor center = new TranslateExecutor(_currentFictionObject, _maxProcessingCount, mergeLimit, ((WorkingThread)sender));
            center.ProcessedLinkList = _processedLinkList;
            center.SaveChinese = chkChinese.Checked;
            center.SingleFile = chkSingleFile.Checked;
            center.Start();
            center.WaitProcess = waitProcess;
            while (!center.Finish())
            {
                waitProcess.WaitOne();
                while (_processingQueue.Count > 0)
                {
                    string downloadChineseText = _processingQueue.Dequeue();
                    center.Translate(downloadChineseText);
                }

            }
           
            _processingQueue.Clear();
            Console.WriteLine(_currentFictionObject.Name + " translated completely");
        }

        #region backup
        /*
         private void FictionTranslate(object sender)
        {

            long mergeLimit = Int32.Parse(txtLimit.Text);
            //bool dontNeedTranslate = false;
            while (_currentProcessingCount < _maxProcessingCount)
            {
                waitProcess.WaitOne();
                while (_processingQueue.Count > 0)
                {
                    string original = _processingQueue.Dequeue();
                    if (_currentBookCount == 0)
                    {
                        _contentBuilder = new StringBuilder();
                        _originalBuilder = new StringBuilder();
                        _threeTypeBuilder = new StringBuilder();
                    }
                    
                    if (_contentBuilder == null) _contentBuilder = new StringBuilder();
                        original = Utility.StripHTML(original);
                        //original = Utility.StripTagsCharArray(original);

                    if (chkChinese.Checked)
                    {
                        _originalBuilder.Append(original);
                    }
                    string result;

                    if (!original.StartsWith("VPBOBBIE"))
                        result = Translate(original, sender);
                    else
                        result = original.Substring(8);

                    _contentBuilder.Append(result);
                    _currentBookCount += 1;
                    if (_currentBookCount == mergeLimit)
                    {  
                        SaveToDisk(_contentBuilder.ToString(), _stepCount++);

                        if(chkChinese.Checked)
                        {
                            SaveChineseToDisk(_originalBuilder.ToString(), _stepCount);
                        }

                        _currentBookCount = 0;
                        _contentBuilder = null;

                        int loop = (int)Math.Min(_processedLinkList.Count, mergeLimit);

                        while (loop > 0)
                        {
                            _currentFictionObject.FilesList.Add(_processedLinkList[0]);
                            _processedLinkList.RemoveAt(0);
                            loop--;
                        }
                    }
                    _currentProcessingCount++;
                    //((BackgroundWorker)sender).ReportProgress(_currentProcessingCount);
                    ((WorkingThread)sender).ReportProgress(_currentProcessingCount);
                }

            }

            if (_contentBuilder != null)
            {                
                SaveToDisk(_contentBuilder.ToString(), _stepCount++);
                if (chkChinese.Checked)
                {
                    SaveChineseToDisk(_originalBuilder.ToString(), _stepCount);
                }
                if (chkSingleFile.Checked)
                {
                    string dir = txtLocation.Text.Trim();
                    String zipFile = dir + @"\" + _currentFictionObject.Name + ".zip";
                    int step = _currentFictionObject.GetPreviousStepCount() < 1 ? 1 : _currentFictionObject.GetPreviousStepCount();
                    string path = dir + "\\" + string.Format("{0:000}", step) + ".txt";
                    if (File.Exists(path))
                    {
                        if (File.Exists(zipFile))
                        {
                            File.Delete(zipFile);
                        }
                        using (ZipFile zip = new ZipFile())
                        {
                            zip.AddFile(path,"");
                            zip.Save(zipFile);
                        }
                        File.Delete(path);
                    }
                }
                ((WorkingThread)sender).ReportProgress(_currentProcessingCount);
                _contentBuilder = null;
                _currentBookCount = 0;
                _currentFictionObject.FilesList.AddRange(_processedLinkList);
                _processedLinkList.Clear();
                _processingQueue.Clear();

            }
        }*/
        #endregion backup
        /// <summary>
        /// Saves the chinese to disk.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="stepCount">The step count.</param>
        private void SaveChineseToDisk(string content, int stepCount)
        {
            string dir = txtLocation.Text.Trim() + @"\\Chinese";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string path = dir + "\\" + string.Format("{0:000}", stepCount) + ".txt";
            //File.Create(path);
            File.WriteAllText(path, content, Encoding.UTF8); 
        }

        private string Clean(string content)
        {
            content = System.Text.RegularExpressions.Regex.Replace(content,
                    @"[\!\*|\-|\@|\#|\?|\=|\~|\%|\#|\|*|\@|\?|\&|\^|\$|\^|\-|\@|\^|\+|\%]{5}.*[\!\*|\-|\@|\#|\?|\=|\~|\%|\#|\|*|\@|\?|\&|\^|\$|\^|\-|\@|\^|\+|\%]{5}",
                    "", RegexOptions.IgnoreCase);

            content = content.Replace(@"quyển sách tung hoành Trung văn lưới thủ phát ,hoan nghênh đọc giả đăng lục www.zongheng.comxem xét càng nhiều  vĩ đại  tác phẩm .", "");
            content = content.Replace(@"ngài đích trương mục hơn ngạch vì :0cá tung hoành tệ ta nếu sung trị giá |miễn phí thu được  tung hoành tệ   100tung hoành tệ  366tung hoành tệ  666tung hoành tệ  888tung hoành tệ  3666tung hoành tệ  6666tung hoành tệ  8888tung hoành tệ  10000tung hoành tệ  66666tung hoành tệ  100000tung hoành tệ", "");
            content = content.Replace("\t\t\t", "");
            content = content.Replace("\n\n\n", "\n\n");
            return content;
        }



        /// <summary>
        /// Saves to disk.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="stepCount">The step count.</param>
        private void SaveToDisk(string content,int stepCount)
        {
            if (_currentFictionObject.HTMLLink.IndexOf("zongheng") >= 0)
            {
                content = Clean(content);
            }

            string dir = txtLocation.Text.Trim();
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir); 
            }
            
            if (chkSingleFile.Checked)
            {
                int step = _currentFictionObject.PreviousStepCount;
                if (step < 1) step = 1;
                string path = dir + "\\" + string.Format("{0:000}", step) + ".txt";
                if (File.Exists(path))
                {
                    File.AppendAllText(path,content,Encoding.UTF8);
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

        /// <summary>
        /// Processes the content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        private string ProcessContent(string content)
        {
            string strippedContent  = Utility.StripHTML(content);
            //strippedContent = Translate(strippedContent);
            return strippedContent;

        }

        /// <summary>
        /// Translates the specified original content.
        /// </summary>
        /// <param name="originalContent">Content of the original.</param>
        /// <param name="sender">The sender.</param>
        /// <returns></returns>
        private string Translate(string originalContent,object sender)
        {
            string translateContent = originalContent;
            StringBuilder content = new StringBuilder(translateContent);
            string status ="VANBAN:" + content.Length.ToString() + " characters -- ";
            long curTick=DateTime.Now.Ticks;
            //translateContent = Regex.Replace(translateContent, GlobalCache.VietPhrasePattern,
            //                                 m => GlobalCache.VietPhrase[m.Value]+" ");            
            GlobalCache.VietPhrase.AsEnumerable().ToList().ForEach(t =>
                translateContent = VBStrings.Replace(translateContent,t.Key, t.Value + " "));
            GlobalCache.Names.AsEnumerable().ToList().ForEach(t =>
                translateContent = VBStrings.Replace(translateContent, t.Key, t.Value + " "));
            GlobalCache.ChinesePhienAmWords.AsEnumerable().ToList().ForEach(t =>
                translateContent = VBStrings.Replace(translateContent, t.Key, t.Value + " "));
            GlobalCache.ThanhNgu.AsEnumerable().ToList().ForEach(t =>
                translateContent = VBStrings.Replace(translateContent, t.Key, t.Value + " "));

            /*GlobalCache.VietPhrase.AsEnumerable().ToList().ForEach(t => content.Replace(t.Key, t.Value + " "));
            GlobalCache.Names.AsEnumerable().ToList().ForEach(t => content.Replace(t.Key, t.Value + " "));
            GlobalCache.ChinesePhienAmWords.AsEnumerable().ToList().ForEach(t => content.Replace(t.Key, t.Value + " "));
            GlobalCache.ThanhNgu.AsEnumerable().ToList().ForEach(t => content.Replace(t.Key, t.Value + " "));*/

        long endTick = DateTime.Now.Ticks - curTick;
            ((WorkingThread)sender).ReportProgress(0, status + "TRANSLATED:" + (endTick / 10000000).ToString() + "s");
            return translateContent;            
        }

        /// <summary>
        /// Translates the specified original content.
        /// </summary>
        /// <param name="originalContent">Content of the original.</param>
        /// <returns></returns>
        private string Translate(string originalContent)
        {
            StringBuilder content = new StringBuilder(originalContent);

            GlobalCache.VietPhrase.AsEnumerable().ToList().ForEach(t => content.Replace(t.Key, t.Value + " "));
            GlobalCache.Names.AsEnumerable().ToList().ForEach(t => content.Replace(t.Key, t.Value + " "));
            GlobalCache.ChinesePhienAmWords.AsEnumerable().ToList().ForEach(t => content.Replace(t.Key, t.Value + " "));
            GlobalCache.ThanhNgu.AsEnumerable().ToList().ForEach(t => content.Replace(t.Key, t.Value + " "));
            #region unused
            //foreach (KeyValuePair<string, string> pair in GlobalCache.VietPhrase)
            //{
            //    //content = content.Replace(pair.Key, pair.Value + " ");
            //    content.Replace(pair.Key, pair.Value + " ");
            //}

            //foreach (KeyValuePair<string, string> pair in GlobalCache.Names)
            //{
            //    content.Replace(pair.Key, pair.Value + " ");
            //}

            //foreach (KeyValuePair<string, string> pair in GlobalCache.ChinesePhienAmWords)
            //{
            //    content.Replace(pair.Key, pair.Value + " ");
            //}

            //foreach (KeyValuePair<string, string> pair in GlobalCache.ThanhNgu)
            //{
            //    content.Replace(pair.Key, pair.Value + " ");
            //}
            #endregion
            return content.ToString();
        }

        /// <summary>
        /// Determines whether the specified content has found.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="phrase">The phrase.</param>
        /// <returns>
        ///   <c>true</c> if the specified content has found; otherwise, <c>false</c>.
        /// </returns>
        private bool HasFound(string content, string phrase)
        {
            return content.IndexOf(phrase) >= 0 ? true : false;
        }

        /// <summary>
        /// Creates the new fiction object.
        /// </summary>
        /// <returns></returns>
        private FictionObject CreateNewFictionObject()
        {
            FictionObject fictionObject = new FictionObject();
            fictionObject.HTMLLink = txtHTMLLink.Text.Trim();
            fictionObject.Location = txtLocation.Text.Trim();
            fictionObject.Name = txtName.Text.Trim();
            fictionObject.FilesList = new List<string>();
            fictionObject.NewFilesList = new List<string>();
            fictionObject.Author = txtAuthor.Text.Trim();

            _fictionObjectManager.Add(fictionObject);
            dgvFictions.CurrentCell = dgvFictions[0, _fictionObjectManager.ProjectList.Count - 1];
            dgvFictions.Refresh();
            return fictionObject;
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Close();
            Application.Exit();
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            txtLocation.Text = Setting.Default.Workspace + "\\" + txtName.Text.Trim();
        }

        /// <summary>
        /// Handles the Load event of the MainForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadFictionObjects();
            
        }

        private void LoadFictionObjects()
        {
            _fictionObjectManager = FictionObjectManager.Instance;
            _fictionObjectManager.Init();

            fictionObjectBindingSource.DataSource = _fictionObjectManager.ProjectList;
            fictionObjectBindingSource.ResetBindings(false);
            RefreshDataGrid();
            grpFictionInformation.Enabled = false;
        }

        private void RefreshDataGrid()
        {
            dgvFictions.ResetBindings();
            dgvFictions.Refresh();
            dgvFictions.Invalidate();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DataGridViewSelectedRowCollection selection = dgvFictions.SelectedRows;
            List<FictionObject> list = FictionObjectManager.Instance.ProjectList;

            List<int> removeList = new List<int>();
            foreach (DataGridViewRow row in selection)
            {
                removeList.Add(row.Index);
            }

            int count = removeList.Count - 1;
            while(count >= 0)
            {
                list.RemoveAt(removeList[count]);
                count--;
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the dataGridView1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if(dgvFictions.CurrentRow == null) return;
            _currentFictionObject = FictionObjectManager.Instance.ProjectList[dgvFictions.CurrentRow.Index];
            LoadFictionObjectToForm();
        }

        /// <summary>
        /// Loads the fiction object to form.
        /// </summary>
        private void LoadFictionObjectToForm()
        {
            txtName.Text = _currentFictionObject.Name;
            txtHTMLLink.Text = _currentFictionObject.HTMLLink;
            // txtLocation.Text = _currentFictionObject.Location;
            if(_currentFictionObject.Location.StartsWith(@"\")) _currentFictionObject.Location = _currentFictionObject.Location.Substring(1);
            txtLocation.Text = Setting.Default.Workspace + @"\" + _currentFictionObject.Location;
            txtUpdateSign.Text = _currentFictionObject.UpdateSignString;
            txtAuthor.Text = _currentFictionObject.Author;
            chkGoogleTranslate.Checked = _currentFictionObject.UseGoogleMobile;
            chkUseVPBot.Checked = _currentFictionObject.UseVpBotbie;
            if (_currentFictionObject.UpdateSign == UpdateSignType.Incremental)
            {
                rdoIncremental.Checked = true;
            }
            else if (_currentFictionObject.UpdateSign == UpdateSignType.StringPrefix)
            {
                rdoStringPrefix.Checked = true;
            }
            else if (_currentFictionObject.UpdateSign == UpdateSignType.RegExp)
            {
                rdoRegExp.Checked = true;
            }
            else
            {
                rdoNone.Checked = true;
            }
            chkSort.Checked = _currentFictionObject.SortBeforeDownload;
            txtPreviousStepCount.Text = _currentFictionObject.PreviousStepCount.ToString();

        }

        /// <summary>
        /// Saves the form to fiction object.
        /// </summary>
        private void SaveFormToFictionObject()
        {
            _currentFictionObject.Name = txtName.Text;
            _currentFictionObject.HTMLLink = txtHTMLLink.Text;
            _currentFictionObject.Location=txtLocation.Text;
            if (_currentFictionObject.Location.StartsWith(Setting.Default.Workspace))
            {
                int start = _currentFictionObject.Location.LastIndexOf(@"\");
                _currentFictionObject.Location = _currentFictionObject.Location.Substring(start);
            }
            if (rdoIncremental.Checked) _currentFictionObject.UpdateSign = UpdateSignType.Incremental;
            if(rdoStringPrefix.Checked)
            {
                _currentFictionObject.UpdateSign = UpdateSignType.StringPrefix;
                _currentFictionObject.UpdateSignString = txtUpdateSign.Text.Trim();
            }
            if(rdoRegExp.Checked)
            {
                _currentFictionObject.UpdateSign = UpdateSignType.RegExp;
                _currentFictionObject.UpdateSignString = txtUpdateSign.Text.Trim();    
            }
            if(rdoNone.Checked)
            {
                _currentFictionObject.UpdateSign = UpdateSignType.None;
            }
            _currentFictionObject.Author = txtAuthor.Text;
            _currentFictionObject.UseGoogleMobile = chkGoogleTranslate.Checked;
            _currentFictionObject.UseVpBotbie = chkUseVPBot.Checked;
            int prevCount = 0;
            Utility.TryActionHelper(delegate() { prevCount = int.Parse(txtPreviousStepCount.Text); }, 1);
            _currentFictionObject.PreviousStepCount = prevCount;
            _currentFictionObject.SortBeforeDownload = chkSort.Checked;
            _currentFictionObject.Save();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            FictionObject fictionObject = CreateNewFictionObject();
            _currentFictionObject = fictionObject;
            _currentFictionObject.Clear();
            _processingQueue.Clear();

            LockInformation(ADD_MODE);
            ClearControls();
            Refresh();
        }

        /// <summary>
        /// Locks the information.
        /// </summary>
        /// <param name="mode">The mode.</param>
        private void LockInformation(int mode)
        {
            _currentMode = mode;
            switch (_currentMode)
            {
                case VIEW_MODE:
                    grpFictionInformation.Enabled = false;
                    grpStatus.Enabled = true;
                    dgvFictions.Enabled = true;
                    btnUpdate.Enabled = true;
                    btnUpdateAll.Enabled = true;
                    btnAdd.Enabled = true;
                    btnEdit.Enabled = true;
                    btnDelete.Enabled = true;
                    btnStart.Enabled = true;
                    btnStartAll.Enabled = true;
                    btnStop.Enabled = false;
                    break;
                case EDIT_MODE:
                    grpFictionInformation.Enabled = true;
                    grpStatus.Enabled = true;
                    dgvFictions.Enabled = false;
                    btnUpdate.Enabled = true;
                    btnUpdateAll.Enabled = true;
                    btnAdd.Enabled = false;
                    btnEdit.Enabled = false;
                    btnDelete.Enabled = false;
                    btnStart.Enabled = true;
                    btnStartAll.Enabled = true;
                    btnStop.Enabled = false;
                    break;
                case ADD_MODE:
                    grpFictionInformation.Enabled = true;
                    grpStatus.Enabled = true;
                    dgvFictions.Enabled = true;
                    btnUpdate.Enabled = false;
                    btnUpdateAll.Enabled = false;
                    btnAdd.Enabled = false;
                    btnEdit.Enabled = false;
                    btnDelete.Enabled = false;
                    btnStart.Enabled = true;
                    btnStartAll.Enabled = false;
                    btnStop.Enabled = false;
                    break;
                case START_JOB_MODE:
                    grpFictionInformation.Enabled = false;
                    grpStatus.Enabled = true;
                    dgvFictions.Enabled = true;
                    btnUpdate.Enabled = false;
                    btnUpdateAll.Enabled = false;
                    btnAdd.Enabled = false;
                    btnEdit.Enabled = false;
                    btnDelete.Enabled = false;
                    btnStart.Enabled = false;
                    btnStartAll.Enabled = false;
                    btnStop.Enabled = true;
                    break;
                default:
                    break;
            }
            
        }

        /// <summary>
        /// Clears the controls.
        /// </summary>
        private void ClearControls()
        {
            txtName.Text = "";
            txtLocation.Text = "";
            txtHTMLLink.Text = "";
            txtUpdateSign.Text = "";
            rdoNone.Checked = true;
        }

        /// <summary>
        /// Handles the Click event of the btnUpdate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if(dgvFictions.CurrentRow == null) return;
            BackgroundWorker singleUpdateWorker = new BackgroundWorker();
            singleUpdateWorker.DoWork += new DoWorkEventHandler(singleUpdateWorker_DoWork);
            singleUpdateWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(singleUpdateWorker_RunWorkerCompleted);
            singleUpdateWorker.WorkerReportsProgress = true;
            singleUpdateWorker.ProgressChanged += new ProgressChangedEventHandler(singleUpdateWorker_ProgressChanged);

            btnUpdate.Enabled = false;
            btnUpdateAll.Enabled = false;
            singleUpdateWorker.RunWorkerAsync();
            timer1.Start();
        }

        /// <summary>
        /// Handles the ProgressChanged event of the singleUpdateWorker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.ProgressChangedEventArgs"/> instance containing the event data.</param>
        void singleUpdateWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lblUpdate.Text = " Đang update " + e.UserState.ToString();
            fictionObjectBindingSource.ResetBindings(false);
            dgvFictions.Refresh();
            dgvFictions.Invalidate();
        }

        void singleUpdateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnUpdate.Enabled = true;
            btnUpdateAll.Enabled = true;
            fictionObjectBindingSource.ResetBindings(false);
            dgvFictions.Refresh();
            dgvFictions.Invalidate();
            timer1.Stop();
            _currentFictionObject.Save();
        }

        void singleUpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ((BackgroundWorker)sender).ReportProgress(0, _currentFictionObject.Name);
            _currentFictionObject.Update();
        }

        void updateWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lblUpdate.Text = " Đang update " + e.ProgressPercentage + "/" + _fictionObjectManager.ProjectList.Count + " : " + (string)e.UserState;
            fictionObjectBindingSource.ResetBindings(false);
            dgvFictions.Refresh();
            dgvFictions.Invalidate();
        }

        void updateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnUpdate.Enabled = true;
            btnUpdateAll.Enabled = true;
            fictionObjectBindingSource.ResetBindings(false);
            dgvFictions.Refresh();
            dgvFictions.Invalidate();
            timer1.Stop();
            _fictionObjectManager.Save();
        }

        void updateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //_fictionObjectManager.Update();
            _fictionObjectManager.Update((BackgroundWorker)sender);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            LockInformation(EDIT_MODE);
        }

        private void BtnSaveFictionObjectClick(object sender, EventArgs e)
        {
            // check error on GUI before starting
            if (!CheckIntegrity()) return;
            SaveFormToFictionObject();
            _currentFictionObject.Save();
            // check and delete file            
            if (_currentFictionObject.ResetFlag)
            {
                int count = 1;
                string fictionPath = Setting.Default.Workspace + "\\" + _currentFictionObject.Location;
                String zipFile = fictionPath + @"\" + _currentFictionObject.Name + ".zip";
                if (File.Exists(zipFile)) File.Delete(zipFile);
                string firstFile = fictionPath + "\\" + string.Format("{0:000}", count++) + ".txt";
                if (File.Exists(firstFile)) File.Delete(firstFile);
                string nextFile = fictionPath + "\\" + string.Format("{0:000}", count++) + ".txt";
                while (File.Exists(nextFile))
                {
                    File.Delete(nextFile);
                    nextFile = fictionPath + "\\" + string.Format("{0:000}", count++) + ".txt";
                }
                if(File.Exists(fictionPath + "\\mykindlebook.opf")) File.Delete(fictionPath + "\\mykindlebook.opf");
                if (File.Exists(fictionPath + "\\mykindlebook.html")) File.Delete(fictionPath + "\\mykindlebook.html");
                if (File.Exists(fictionPath + "\\toc.ncx")) File.Delete(fictionPath + "\\toc.ncx");
                if (File.Exists(fictionPath + "\\toc.html")) File.Delete(fictionPath + "\\toc.html");
                _currentFictionObject.ResetFlag = false;
            }            

            if (sender != null) InformationBox.Show("Save OK!", new AutoCloseParameters(1));
            
            switch(_currentMode)
            {
                case ADD_MODE:
                    _fictionObjectManager.ProjectList.Add(_currentFictionObject);
                    //IEnumerable<FictionObject> source = _fictionObjectManager.ProjectList.OfType<FictionObject>();
                    //var sorted = (   from item in source
                    //                 orderby item.Name ascending 
                    //                 select item).ToList();
                    //_fictionObjectManager.ProjectList = sorted.ToList();
                    //fictionObjectBindingSource.DataSource = _fictionObjectManager.ProjectList;
                    fictionObjectBindingSource.ResetBindings(false);
                    RefreshDataGrid();
                    
                    break;
                case EDIT_MODE:
                    fictionObjectBindingSource.ResetBindings(false);
                    RefreshDataGrid();
                    break;
                default:
                    break;
            }
            LockInformation(VIEW_MODE);            
        }

        private void rdoIncremental_CheckedChanged(object sender, EventArgs e)
        {
            _currentFictionObject.UpdateSign = UpdateSignType.Incremental;
            if (!rdoIncremental.Checked && !rdoNone.Checked)
            {
                txtUpdateSign.Enabled = true;
            }
            else
            {
                txtUpdateSign.Enabled = false;
            }
        }

        private void rdoStringPrefix_CheckedChanged(object sender, EventArgs e)
        {
            _currentFictionObject.UpdateSign = UpdateSignType.StringPrefix;
            if (!rdoIncremental.Checked && !rdoNone.Checked)
            {
                txtUpdateSign.Enabled = true;
            }
            else
            {
                txtUpdateSign.Enabled = false;
            }
        }

        private void rdoRegExp_CheckedChanged(object sender, EventArgs e)
        {
            _currentFictionObject.UpdateSign = UpdateSignType.RegExp;
            if (!rdoIncremental.Checked && !rdoNone.Checked)
            {
                txtUpdateSign.Enabled = true;
            }
            else
            {
                txtUpdateSign.Enabled = false;
            }
        }

        private void rdoNone_CheckedChanged(object sender, EventArgs e)
        {
            _currentFictionObject.UpdateSign = UpdateSignType.None;
            if (!rdoIncremental.Checked && !rdoNone.Checked)
            {
                txtUpdateSign.Enabled = true;
            }
            else
            {
                txtUpdateSign.Enabled = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Bạn có chắc bạn muốn xóa hết cache ?","Xác nhận", MessageBoxButtons.YesNo);
            if(dialogResult == DialogResult.No)
            {
                return;
            }
            if(_currentFictionObject!=null)
            {
                _currentFictionObject.FilesList.Clear();
                _currentFictionObject.PreviousStepCount = 0;
            }
            _currentFictionObject.Reset();
            dgvFictions.Refresh();
            dgvFictions.Invalidate();
        }

        /// <summary>
        /// Handles the FormClosing event of the MainForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.FormClosingEventArgs"/> instance containing the event data.</param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(_currentFictionObject!= null ) _currentFictionObject.Save();
            StopAllThreads();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblUpdate.Text += ".";
            if(lblUpdate.Text.EndsWith("......"))
            {
                lblUpdate.Text = lblUpdate.Text.Replace("......", ".");
            }
        }

        private void btnUpdateAll_Click(object sender, EventArgs e)
        {
            BackgroundWorker updateWorker = new BackgroundWorker();
            updateWorker.DoWork += updateWorker_DoWork;
            updateWorker.RunWorkerCompleted += updateWorker_RunWorkerCompleted;
            updateWorker.WorkerReportsProgress = true;
            updateWorker.ProgressChanged += updateWorker_ProgressChanged;

            btnUpdate.Enabled = false;
            btnUpdateAll.Enabled = false;
            updateWorker.RunWorkerAsync();
            timer1.Start();
        }

        private void btnStartAll_Click(object sender, EventArgs e)
        {
            LockInformation(START_JOB_MODE);
            grandProcessQueue = new Queue<int>();
            foreach (DataGridViewRow row in dgvFictions.Rows)
            {
                grandProcessQueue.Enqueue(row.Index);
            }
            
            CheckGrandQueue();
        }

        private void dgvFictions_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // do nothing in here
        }

        private void cấuHìnhToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new SettingForm().Show();
        }

        private void txtUpdateSign_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnStop_Click(object sender, EventArgs e)
        {

        }

        private void ChangeNameWebsiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ChangeNameSiteForm().ShowDialog();
            this.OnLoad(null);
        }

        /// <summary>
        /// Handles the Click event of the LinkListToolStripMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void LinkListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_currentFictionObject == null) return;
            List<string> linkList = _currentFictionObject.FilesList;
            StringBuilder builder = new StringBuilder();
            foreach (string link in linkList)
            {
                builder.AppendLine(link);
            }
            LinkListForm form = new LinkListForm();
            form.LinkList = builder.ToString();
            form.ShowDialog();
            string result = form.LinkList;
            List<string> list = new List<string>(result.Split('\n'));
            DialogResult dialogResult = MessageBox.Show("Ban muon luu ket qua lai khong ?", "Warning", MessageBoxButtons.YesNo);
            // if yes so save data back
            if(dialogResult == DialogResult.Yes) _currentFictionObject.FilesList = list;
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            LockInformation(VIEW_MODE);
        }

        private void capNhatEncodingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (FictionObject fictionObject in _fictionObjectManager.ProjectList)
            {
                if (Utility.IsUtf8Site(fictionObject.HTMLLink))
                {
                    fictionObject.ContentEncoding = Encoding.UTF8;
                    fictionObject.Save();
                    continue;
                }
                WebClient webClient = new WebClient();
                Encoding chineseEncoding = null;
                webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                
                    // try to get encoding from web site
                try
                {
                    byte[] data = webClient.DownloadData(fictionObject.HTMLLink);
                }
                catch(Exception ex)
                {
                    fictionObject.ContentEncoding = Encoding.GetEncoding("GB2312");
                    fictionObject.Save();
                    continue;
                }
                string pageEncodingString = webClient.ResponseHeaders["Content-Encoding"];
                    // check whether we can get a valid encoding ??
                    if (string.IsNullOrEmpty(pageEncodingString))
                    {
                        pageEncodingString = webClient.ResponseHeaders["Content-Type"];
                        if (!string.IsNullOrEmpty(pageEncodingString)
                             && pageEncodingString.IndexOf("charset=", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            pageEncodingString =
                                pageEncodingString.Substring(
                                    pageEncodingString.IndexOf("charset=", StringComparison.OrdinalIgnoreCase) + 8);
                        }
                        else
                        {
                            pageEncodingString = null;
                        }
                    }

                    if (!string.IsNullOrEmpty(pageEncodingString)) chineseEncoding = Encoding.GetEncoding(pageEncodingString);
                    else
                        chineseEncoding = Encoding.GetEncoding("GB2312");
                fictionObject.ContentEncoding = chineseEncoding;
                fictionObject.Save();
            }
        }

        private void loadLaiThuMucToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadFictionObjects();
        }

        private void singleFleTranslateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TranslateSingleForm form = new TranslateSingleForm();
            form.Show(this);
        }

        private void mergeFileStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (FictionObject fictionObject in _fictionObjectManager.ProjectList)
            {
                int count = 1;
                string fictionPath = Setting.Default.Workspace + "\\" + fictionObject.Location;
                string firstFile = fictionPath + "\\" + string.Format("{0:000}", count++) + ".txt";
                if(!File.Exists(firstFile)) continue;
                string nextFile = fictionPath + "\\" + string.Format("{0:000}", count++) + ".txt";
                while (File.Exists(nextFile))
                {
                    String content = File.ReadAllText(nextFile, Encoding.UTF8);
                    File.AppendAllText(firstFile,content,Encoding.UTF8);
                    File.Delete(nextFile);
                    nextFile = fictionPath + "\\" + string.Format("{0:000}", count++) + ".txt";
                }
            }
        }

        private void taoFileZipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (FictionObject fictionObject in _fictionObjectManager.ProjectList)
            {   
                int count = 1;
                string fictionPath = Setting.Default.Workspace + "\\" + fictionObject.Location;
                String zipFile = fictionPath + @"\" + fictionObject.Name + ".zip";
                string[] files = Directory.GetFiles(fictionPath, "*.zip");        
                if(files.Length > 0)
                {
                    string firstZipFile = files[0];
                    if(!firstZipFile.Equals(zipFile))
                    {
                        File.Move(firstZipFile, zipFile);
                    }
                }        
                string firstFile = fictionPath + "\\" + string.Format("{0:000}", count++) + ".txt";
                if (!File.Exists(firstFile)) continue;
                string nextFile = fictionPath + "\\" + string.Format("{0:000}", count++) + ".txt";
                if (File.Exists(nextFile)) continue; // Files are not merged so don't create zip
                if (File.Exists(zipFile)) File.Delete(zipFile);
                using (ZipFile zip = new ZipFile())
                {
                    zip.AddFile(firstFile, "");
                    zip.Save(zipFile);
                }
                File.Delete(firstFile);

            }
            InfoBox.InformationBox.Show("Hoan tat !", new AutoCloseParameters(2));
        }

        private void createMoreFileEpubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TranslateExecutor center = new TranslateExecutor(_currentFictionObject, _currentFictionObject.ChapterCount, 2000, null);
            center.SaveMultipleEpub();
        }

        private void taoMotFileEbookToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TranslateExecutor center = new TranslateExecutor(_currentFictionObject, _currentFictionObject.ChapterCount, 2000, null);
            center.ConvertToKindle();
        }

        private void cleanKindleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TranslateExecutor center = new TranslateExecutor(_currentFictionObject, _currentFictionObject.ChapterCount, 2000, null);
            center.ClearKindleFile();
        }
    }
}
