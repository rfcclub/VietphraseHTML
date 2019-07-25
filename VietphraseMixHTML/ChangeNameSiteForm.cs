using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InfoBox;

namespace VietphraseMixHTML
{
    public partial class ChangeNameSiteForm : Form
    {
        public ChangeNameSiteForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string originalName = textBoxOriginalSite.Text.Trim();
            string newName = textBoxNewSite.Text.Trim();
            if (string.IsNullOrEmpty(originalName) || string.IsNullOrEmpty(newName)) return;
            FictionObjectManager manager = FictionObjectManager.Instance;
            foreach(FictionObject fictionObj in manager.ProjectList)
            {
                if(fictionObj.HTMLLink.StartsWith(originalName))
                {
                    string htmlLink = fictionObj.HTMLLink;
                    htmlLink = htmlLink.Replace(originalName, newName);
                    fictionObj.HTMLLink=htmlLink;
                }
                else if (fictionObj.HTMLLink.StartsWith(newName))
                {
                    IList<string> list = new List<string>();
                    foreach (string link in fictionObj.FilesList)
                    {
                        string newLink = link;
                        if (link.StartsWith(originalName))
                        {
                            newLink = link.Replace(originalName, newName);
                        }
                        list.Add(newLink);
                    }
                    fictionObj.FilesList.Clear();
                    fictionObj.FilesList.AddRange(list);
                }
            }
            manager.Save();
            InformationBox.Show("Change name OK !", new AutoCloseParameters(1));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
