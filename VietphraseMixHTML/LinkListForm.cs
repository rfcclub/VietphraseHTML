using System;
using System.Windows.Forms;

namespace VietphraseMixHTML
{
    public partial class LinkListForm : Form
    {
        public string LinkList
        {
            get { return richTextBox1.Text.Trim(); }
            set { richTextBox1.Text = value; }
        }
        public LinkListForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(richTextBox1.Text);
        }
    }
}
