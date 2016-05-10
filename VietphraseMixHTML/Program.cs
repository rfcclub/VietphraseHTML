using System;
using System.Collections.Generic;
using System.Linq;
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

            

            string dir = Application.StartupPath;
            GlobalCache.Init(dir);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (string.IsNullOrEmpty(Setting.Default.Workspace))
            {
                new SettingForm().ShowDialog();
            }

            Application.Run(new MainForm());
            
        }
    }
}
