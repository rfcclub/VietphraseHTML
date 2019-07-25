using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Windows.Forms;
using HtmlAgilityPack;
using InfoBox;

namespace VietphraseMixHTML
{
    public partial class GetLinksForm : Form
    {
        public string Url { get; set; }

        public List<string> UrlList { get; set; }
        public List<string> ChapterNameList { get; set; }
        public string UpdateSignString { get; set; }
        public UpdateSignType UpdateSign { get; set; }

        public bool SortBeforeDownload { get; set; }
        public Encoding PageEncoding { get; set; }

        public Dictionary<string,string> LinksList { get; set; }
        public GetLinksForm()
        {
            InitializeComponent();
            LinksList = new Dictionary<string, string>();
        }

        private string content = "";
        private List<string>_sortUrlList = new List<string>();

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

        public void GetLinks()
        {
            lstLinks.Items.Clear();
            HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            //Trust all certificates
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            WebClient client = new WebClient();
            // request.Timeout = 60*10*1000;
            /* HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            long contentLength = response.ContentLength;
            byte[] buff = new byte[contentLength];
            response.GetResponseStream().Read(buff, 0, (int)contentLength);
            response.GetResponseStream().Flush();
            response.GetResponseStream().Close();*/
            Encoding pageEncoding = Encoding.GetEncoding("GB2312");
            if (IsUTF8Site(GlobalCache.UTF8Sites, Url)) pageEncoding = Encoding.UTF8;

            try
            {
                htmlDocument.Load(client.OpenRead(Url), pageEncoding);
            }
            catch (Exception ex)
            {
                InfoBox.InformationBox.Show("Error when load URL: " + ex.Message, new AutoCloseParameters(3));
                return;
            }

            PageEncoding = pageEncoding;
            var links = htmlDocument.DocumentNode.SelectNodes("//a");
            if (links == null || links.Count == 0)
            {
                InfoBox.InformationBox.Show("LINKS ARE EMPTY !!!!", new AutoCloseParameters(3));
                
                return;
            }
                foreach (HtmlNode link in links)
                {
                    
                    HtmlAttribute att = link.Attributes["href"];
                    if (att != null)
                    {
                        lstLinks.Items.Add(att.Value);
                        LinksList[att.Value] = link.InnerHtml;
                    }
                    else
                    {
                        // process zongheng link.
                        if (Url.IndexOf("book.zongheng.com") >= 0)
                        {
                            HtmlAttribute att1 = link.Attributes["chapterId"];
                            if (att1 == null) continue;
                            string bookName = Url.Substring(Url.LastIndexOf("/")+1);
                            bookName = bookName.Substring(0,bookName.LastIndexOf("."));
                            string htmllink = "http://book.zongheng.com/chapter/" + bookName + "/" + att1.Value + ".html";
                            lstLinks.Items.Add(htmllink);
                            LinksList[htmllink] = link.InnerHtml;
                        }
            
                    }
                }            

        }

        private void GetLinksForm_Shown(object sender, EventArgs e)
        {
            GetLinks();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(txtUpdateSign.Text))
            {
                UpdateSignString = txtUpdateSign.Text;
            }

            UrlList = new List<string>();
            ChapterNameList = new List<string>();
            bool moveToRoot = false;
            foreach (var item in lstLinks.SelectedItems)
            {
                string url = null;
                url = item.ToString();
                if(!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    
                    if (url.StartsWith("/"))
                    {
                        url = url.Remove(0, 1);
                        moveToRoot = true;
                    }
                    if (url.StartsWith("wenxue"))
                    {
                        url = url.Substring(url.LastIndexOf("/") + 1);
                    }
                    if(!moveToRoot) url = Url.Substring(0, Url.LastIndexOf("/") + 1) + url;
                    else
                    {
                        url = Url.Substring(0, Url.IndexOf("/", 8) + 1) + url;
                        moveToRoot = false;
                    }
                }
                UrlList.Add(url);
                ChapterNameList.Add(LinksList[(string)item]);
            }
            Close();
        }

        private void GetLinksForm_Load(object sender, EventArgs e)
        {
            txtUpdateSign.Text = UpdateSignString;

            if(UpdateSign == UpdateSignType.Incremental)
            {
                rdoIncremental.Checked = true;
            }
            else if (UpdateSign == UpdateSignType.StringPrefix)
            {
                rdoStringPrefix.Checked = true;
            }
            else if (UpdateSign == UpdateSignType.RegExp)
            {
                rdoRegExp.Checked = true;
            }
            else 
            {
                rdoNone.Checked = true;
            }
        }

        private void rdoIncremental_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSign = UpdateSignType.Incremental;
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
            UpdateSign = UpdateSignType.StringPrefix;
            if (!rdoIncremental.Checked && !rdoNone.Checked)
            {
                txtUpdateSign.Enabled = true;
            }
            else
            {
                txtUpdateSign.Enabled = false;
            }
            TryToDetermineStringPrefix();
        }

        private void TryToDetermineStringPrefix()
        {
            IEnumerable<string> source = lstLinks.SelectedItems.OfType<string>();
            var sorted = (from item in source
                          orderby item.Length, item ascending
                          select item).ToList();
            if (sorted.Count > 1)
            {
                string item1 = sorted[0];
                string item2 = sorted[1];
                StringBuilder builder = new StringBuilder();
                int maxLength = item1.Length > item2.Length ? item2.Length : item1.Length;
                for (int i = 0; i < maxLength; i++)
                {
                    if (item1[i] == item2[i])
                    {
                        builder.Append(item1[i]);
                    }
                    else
                    {
                        break;
                    }
                }
                txtUpdateSign.Text = builder.ToString();
            }
        }

        private void rdoRegExp_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSign = UpdateSignType.RegExp;
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
            UpdateSign = UpdateSignType.None;
            if (!rdoIncremental.Checked && !rdoNone.Checked)
            {
                txtUpdateSign.Enabled = true;
            }
            else
            {
                txtUpdateSign.Enabled = false;
            }
        }

        private void chkSort_CheckedChanged(object sender, EventArgs e)
        {
            
            
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            GetLinks();
        }

        private void btnSort_Click(object sender, EventArgs e)
        {
            /*lstLinks.Sorted = true;*/
            _sortUrlList.Clear();
            IEnumerable<string> source = lstLinks.Items.OfType<string>();
            var sorted = (from item in source
                          orderby item.Length, item ascending
                          select item).ToList();
            lstLinks.Items.Clear();
            foreach (string itms in sorted)
            {
                lstLinks.Items.Add(itms);
            }
            lstLinks.Invalidate();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if(chkSortable.Checked)
            {
                btnSort_Click(sender, e);
            }
            switch(UpdateSign)
            {
                case UpdateSignType.Incremental:
                    int temp;
                    for(int i=0;i <lstLinks.Items.Count; i++)
                    {
                        string item = (string)lstLinks.Items[i];
                        string numberPart = item.IndexOf(".") > 0 ? item.Substring(0, item.IndexOf(".")) : "";
                        if(int.TryParse(numberPart, out temp))
                        {
                            lstLinks.SetSelected(i, true);
                        }
                    }
                    break;
                case UpdateSignType.StringPrefix:
                    string sign = txtUpdateSign.Text.Trim();
                    for (int i = 0; i < lstLinks.Items.Count; i++)
                    {
                        string item = (string)lstLinks.Items[i];
                        if(item.StartsWith(sign) && !item.EndsWith(sign))
                        {
                            lstLinks.SetSelected(i, true);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void chkSortable_CheckedChanged(object sender, EventArgs e)
        {
            SortBeforeDownload = chkSortable.Checked;
        }
    }
}
