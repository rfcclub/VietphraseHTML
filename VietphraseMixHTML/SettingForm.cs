using System;
using System.Windows.Forms;
using Setting = VietphraseMixHTML.Properties.Settings;

namespace VietphraseMixHTML
{
    public partial class SettingForm : Form
    {
        public SettingForm()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialog.ShowDialog();

            if(result == DialogResult.OK)
            {
                txtWorkspace.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void SettingForm_Load(object sender, EventArgs e)
        {
            txtWorkspace.Text = Setting.Default.Workspace;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Setting.Default.Workspace = txtWorkspace.Text.Trim();
            Setting.Default.Save();

            Close();

        }
    }
}
