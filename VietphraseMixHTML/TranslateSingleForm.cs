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
            worker.RunWorkerAsync();
            buttonTranslate.Enabled = false;
            
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            InformationBox.Show("Save OK!", new AutoCloseParameters(1));
            buttonTranslate.Enabled = true;
            labelStatus.Text = (string)e.Result;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {               
            e.Result = Translate(fileChosen);
        }

        private string Translate(string p)
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
            long curTick = DateTime.Now.Ticks;
            //translateContent = Regex.Replace(translateContent, GlobalCache.VietPhrasePattern,
            //                                 m => GlobalCache.VietPhrase[m.Value]+" ");            
            GlobalCache.VietPhrase.AsEnumerable().ToList().ForEach(t =>
                translateContent = VBStrings.Replace(translateContent, t.Key, t.Value + " "));
            GlobalCache.Names.AsEnumerable().ToList().ForEach(t =>
                translateContent = VBStrings.Replace(translateContent, t.Key, t.Value + " "));
            GlobalCache.ChinesePhienAmWords.AsEnumerable().ToList().ForEach(t =>
                translateContent = VBStrings.Replace(translateContent, t.Key, t.Value + " "));
            GlobalCache.ThanhNgu.AsEnumerable().ToList().ForEach(t =>
                translateContent = VBStrings.Replace(translateContent, t.Key, t.Value + " "));

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
