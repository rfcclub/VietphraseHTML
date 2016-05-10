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

namespace VietphraseMixHTML
{
    public class TranslateCenter
    {
        public const int MAX_THREADS = 5;
        IList<string> originalStrings;
        IList<string> links;
        IDictionary<int, string> translateMap;        
        int processCount = 0;
        int startPoint = 0;
        int totalCount;
        bool start;
        FictionObject fictionObject;
        WorkingThread reportThread;
        int mergeLimit;
        bool noMoreSignal = false;
        IList<WorkingThread> workingThreads;
        public bool SaveChinese { get; set; }
        public bool SingleFile { get; set; }
        public IList<string> ProcessedLinkList { set; private get; }
        public EventWaitHandle WaitProcess {get; set;}
        private object lockObject = new object();
        public TranslateCenter(FictionObject fictionObject, int totalCount, int mergeLimit, WorkingThread reportThread)
        {
            this.fictionObject = fictionObject;
            this.reportThread = reportThread;
            this.totalCount = totalCount;
            this.mergeLimit = mergeLimit;
            processCount = 0;
            startPoint = 0;
            originalStrings = new List<string>();
            translateMap = new Dictionary<int, string>();
            workingThreads = new List<WorkingThread>();
        }

        public bool Finish()
        {
            return !start || processCount == totalCount;
        }

        public void Start()
        {
            start = true;
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
                lock(lockObject)
                {
                    SaveTranslateFiles();
                }
            }
            reportThread.ReportProgress(translateMap.Keys.Count);
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
                if (nextStop > translatedCount) nextStop = translatedCount;
                for (int i = startPoint; i < nextStop; i++)
                {
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
                String zipFile = dir + @"\" + fictionObject.Name + ".zip";
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
            GlobalCache.VietPhrase.AsEnumerable().ToList().ForEach(t =>
                translateContent = VBStrings.Replace(translateContent, t.Key, t.Value + " "));
            GlobalCache.Names.AsEnumerable().ToList().ForEach(t =>
                translateContent = VBStrings.Replace(translateContent, t.Key, t.Value + " "));
            GlobalCache.ChinesePhienAmWords.AsEnumerable().ToList().ForEach(t =>
                translateContent = VBStrings.Replace(translateContent, t.Key, t.Value + " "));
            GlobalCache.ThanhNgu.AsEnumerable().ToList().ForEach(t =>
                translateContent = VBStrings.Replace(translateContent, t.Key, t.Value + " "));

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
