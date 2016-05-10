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
            FictionObjectManager manager = FictionObjectManager.Instance;
            foreach(FictionObject fictionObj in manager.ProjectList)
            {
                if(fictionObj.HTMLLink.StartsWith(originalName))
                {
                    string htmlLink = fictionObj.HTMLLink;
                    htmlLink = htmlLink.Replace(originalName, newName);
                    fictionObj.HTMLLink=htmlLink;
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
