using InfoBox;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VietphraseMixHTML.Properties;
using Setting = VietphraseMixHTML.Properties.Settings;

namespace VietphraseMixHTML
{
    public partial class NewFictionForm : Form
    {
        FictionObject _currentFictionObject = null;
        private const int EDIT_MODE = 1;
        private const int ADD_MODE = 2;
        private int _currentMode = 2;
         
        public bool IsCancelled { get; set; } = false;
        public NewFictionForm()
        {
            InitializeComponent();
            _currentFictionObject = CreateNewFictionObject();
        }

        public void SetWorkingFictionObject(FictionObject fiction)
        {
            _currentFictionObject = fiction;
            SetMode(EDIT_MODE);
            LoadFictionObjectToForm();
        }
        public FictionObject GetResult()
        {
            return _currentFictionObject;
        }

        public void SetMode(int mode)
        {
            _currentMode = mode;
        }

        public int GetMode()
        {
            return _currentMode;
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

            //_fictionObjectManager.Add(fictionObject);
            //dgvFictions.CurrentCell = dgvFictions[0, _fictionObjectManager.ProjectList.Count - 1];
            //dgvFictions.Refresh();
            return fictionObject;
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            
            txtLocation.Text = Setting.Default.Workspace + "\\" + Utility.NormalizeName(txtName.Text.Trim());
        }

        

        /// <summary>
        /// Checks the integrity.
        /// </summary>
        /// <returns></returns>
        private bool CheckIntegrity()
        {
            if (string.IsNullOrEmpty(txtName.Text))
            {
                return false;
            }
            if (string.IsNullOrEmpty(txtHTMLLink.Text))
            {
                return false;
            }
            if (string.IsNullOrEmpty(txtLocation.Text))
            {
                return false;
            }
            return true;
        }

        private void LoadFictionObjectToForm()
        {
            txtName.Text = _currentFictionObject.Name;
            txtHTMLLink.Text = _currentFictionObject.HTMLLink;
            // txtLocation.Text = _currentFictionObject.Location;
            if (_currentFictionObject.Location.StartsWith(@"\")) _currentFictionObject.Location = _currentFictionObject.Location.Substring(1);
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
            _currentFictionObject.Location = txtLocation.Text;
            if (_currentFictionObject.Location.StartsWith(Setting.Default.Workspace))
            {
                int start = _currentFictionObject.Location.LastIndexOf(@"\");
                _currentFictionObject.Location = _currentFictionObject.Location.Substring(start);
            }
            if (rdoIncremental.Checked) _currentFictionObject.UpdateSign = UpdateSignType.Incremental;
            if (rdoStringPrefix.Checked)
            {
                _currentFictionObject.UpdateSign = UpdateSignType.StringPrefix;
                _currentFictionObject.UpdateSignString = txtUpdateSign.Text.Trim();
            }
            if (rdoRegExp.Checked)
            {
                _currentFictionObject.UpdateSign = UpdateSignType.RegExp;
                _currentFictionObject.UpdateSignString = txtUpdateSign.Text.Trim();
            }
            if (rdoNone.Checked)
            {
                _currentFictionObject.UpdateSign = UpdateSignType.None;
            }
            _currentFictionObject.Author = txtAuthor.Text;
            _currentFictionObject.UseGoogleMobile = chkGoogleTranslate.Checked;
            _currentFictionObject.UseVpBotbie = chkUseVPBot.Checked;
            int prevCount = 0;
            Utility.TryActionHelper(delegate () { prevCount = int.Parse(txtPreviousStepCount.Text); }, 1);
            _currentFictionObject.PreviousStepCount = prevCount;
            _currentFictionObject.SortBeforeDownload = chkSort.Checked;
            _currentFictionObject.Save();
        }

        private void btnSaveFictionObject_Click(object sender, EventArgs e)
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
                String zipFile = fictionPath + @"\" + Utility.NormalizeName(_currentFictionObject.Name) + ".zip";
                if (File.Exists(zipFile)) File.Delete(zipFile);
                string firstFile = fictionPath + "\\" + string.Format("{0:000}", count++) + ".txt";
                if (File.Exists(firstFile)) File.Delete(firstFile);
                string nextFile = fictionPath + "\\" + string.Format("{0:000}", count++) + ".txt";
                while (File.Exists(nextFile))
                {
                    File.Delete(nextFile);
                    nextFile = fictionPath + "\\" + string.Format("{0:000}", count++) + ".txt";
                }
                if (File.Exists(fictionPath + "\\mykindlebook.opf")) File.Delete(fictionPath + "\\mykindlebook.opf");
                if (File.Exists(fictionPath + "\\mykindlebook.html")) File.Delete(fictionPath + "\\mykindlebook.html");
                if (File.Exists(fictionPath + "\\toc.ncx")) File.Delete(fictionPath + "\\toc.ncx");
                if (File.Exists(fictionPath + "\\toc.html")) File.Delete(fictionPath + "\\toc.html");
                _currentFictionObject.ResetFlag = false;
            }

            this.Close();
        }

        private void btnClearCache_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Bạn có chắc bạn muốn xóa hết cache ?", "Xác nhận", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.No)
            {
                return;
            }
            if (_currentFictionObject != null)
            {
                _currentFictionObject.FilesList.Clear();
                _currentFictionObject.PreviousStepCount = 0;
            }
            _currentFictionObject.Reset();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            IsCancelled = true;
            Close();
        }
    }
}
