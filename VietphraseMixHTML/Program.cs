using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Setting = VietphraseMixHTML.Properties.Settings;

namespace VietphraseMixHTML
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            string dir = Application.StartupPath;
            GlobalCache.Init(dir);

            Application.EnableVisualStyles();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(false);

            if (string.IsNullOrEmpty(Setting.Default.Workspace))
            {
                new SettingForm().ShowDialog();
            }

            Application.Run(new MainForm());
            
        }
    }
}
