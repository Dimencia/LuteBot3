using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new LuteBotForm());
            }
            catch (Exception e)
            {
                var saveFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LuteBot");
                var savePath = Path.Combine(saveFolder, "CrashDump_" + DateTime.Now.ToString("MM-dd-yyyy_mm-hh") + ".txt");
                try
                {
                    var currentException = e;
                    string content = $"{DateTime.Now.ToString("G")} - Crash: ";
                    do {
                        content += $"{currentException.Message}\n{currentException.StackTrace}\nIn method {currentException.TargetSite.Name}\nIn class {currentException.TargetSite.ReflectedType.Name}\n\n";
                        currentException = currentException.InnerException;
                    } while (currentException != null);
                    content += "\n\n";
                    Directory.CreateDirectory(saveFolder);
                    File.AppendAllText(savePath, content);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(null, $"Lutebot has crashed: {e.Message}, and could not write a crash report: {ex.Message}\nYou may need to run Lutebot as administrator", "Lutebot has crashed", MessageBoxButtons.OK);
                }
                var result = MessageBox.Show(null, $"Lutebot has crashed: {e.Message}\n\nThe crash report has been saved to {savePath}\n\nWould you like to open the folder?", "Lutebot has crashed", MessageBoxButtons.YesNo);

                if(result == DialogResult.Yes)
                {
                    Process.Start(saveFolder);
                }

                Environment.Exit(1);

            }
        }
    }
}
