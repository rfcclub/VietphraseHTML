using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InfoBox;
using VBStrings = Microsoft.VisualBasic.Strings;

namespace VietphraseMixHTML
{
    public partial class TranslateSingleForm : Form
    {
        string fileChosen = null;
        public TranslateSingleForm()
        {
            InitializeComponent();
        }

        private void buttonSelect_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            DialogResult result = openFile.ShowDialog();
            if (result == DialogResult.OK)
            {
                string fileName = openFile.FileName;
                textChooseFile.Text = fileName;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonTranslate_Click(object sender, EventArgs e)
        {
            fileChosen = textChooseFile.Text;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork+=worker_DoWork;
            worker.RunWorkerCompleted+=worker_RunWorkerCompleted;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.WorkerReportsProgress = true;
            worker.RunWorkerAsync();
            buttonTranslate.Enabled = false;
            
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            labelStatus.Text = (string)e.UserState;
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            InformationBox.Show("Save OK!", new AutoCloseParameters(1));
            buttonTranslate.Enabled = true;
            labelStatus.Text = (string)e.Result;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {               
            e.Result = Translate(fileChosen, (BackgroundWorker)sender);
           
        }

        private string Translate(string p, BackgroundWorker sender)
        {
            if (string.IsNullOrEmpty(p))
            {
                return "NULL path";
            }
            TextReader reader = new StreamReader(p,Encoding.UTF8);
            string content = reader.ReadToEnd();
            reader.Close();

            string translateContent = content;            
            string status = "VANBAN:" + content.Length.ToString() + " characters -- ";
            sender.ReportProgress(10, status);
            long curTick = DateTime.Now.Ticks;
                      
            GlobalCache.VietPhrase.AsEnumerable().ToList().ForEach(t =>
                translateContent = VBStrings.Replace(translateContent, t.Key, t.Value + " "));
            sender.ReportProgress(10, "VietPhrase completed");
            GlobalCache.Names.AsEnumerable().ToList().ForEach(t =>
                translateContent = VBStrings.Replace(translateContent, t.Key, t.Value + " "));
            sender.ReportProgress(10, "Names completed");
            GlobalCache.ChinesePhienAmWords.AsEnumerable().ToList().ForEach(t =>
                translateContent = VBStrings.Replace(translateContent, t.Key, t.Value + " "));
            sender.ReportProgress(10, "ChinesePhienAmWords completed");
            GlobalCache.ThanhNgu.AsEnumerable().ToList().ForEach(t =>
                translateContent = VBStrings.Replace(translateContent, t.Key, t.Value + " "));
            sender.ReportProgress(10, "ThanhNgu completed");

            long endTick = DateTime.Now.Ticks - curTick;            
            //TextWriter writer = new StreamWriter(p,false,Encoding.UTF8);
            FileStream fileStream = new FileStream(p, FileMode.Truncate);
            StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8);
            writer.Write(translateContent);
            writer.Flush();
            writer.Close();

            return status;
        }
    }
}
