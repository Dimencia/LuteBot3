using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace LuteBotUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("The path must be explicitly passed to avoid erasing the wrong files");
                Console.WriteLine(@"Usage: LuteBotUpdater.exe C:\Path\to\LuteBotFolder https://github.com/Dimencia/path/to/LuteBot.zip");
                return;
            }
            string installPath = args[0];
            string downloadUrl = args[1];
            string zipName = "LuteBot.zip";


            try
            {
                // Get the zip file name out
                var zipMatch = Regex.Match(downloadUrl, @"([^\/]*.zip)");
                if (zipMatch.Success)
                    zipName = zipMatch.Groups[1].Value;

                string downloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LuteBot", "UpdaterDownloads");
                if (Directory.Exists(downloadPath))
                    Directory.Delete(downloadPath, true);
                Directory.CreateDirectory(downloadPath);

                downloadPath = Path.Combine(downloadPath, zipName);

                // A thought: Maybe if called without args, it should copy itself to appdata, then call the copy with the args pointing at the folder it was called from
                // But probably not.  It's going to wipe an entire folder, it needs to be a bit careful and require giving the folder explicitly

                // I guess it should maybe, not wipe the folder, and just replace on extract.  That feels risky, but should be OK now that everything we write is in appdata
                // And is less risky than just wiping an arbitrary folder

                // LuteBot should call this and immediately close itself
                Console.WriteLine("Waiting for LuteBot to close");
                var lutebotProcesses = Process.GetProcessesByName("LuteBot");
                foreach (var p in lutebotProcesses)
                    p.WaitForExit();

                // Setup/Clear a downloads folder


                Console.WriteLine($"Downloading zip file from {downloadUrl}");
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(downloadUrl, downloadPath);
                }
                Console.WriteLine($"Extracting zip file to {installPath}");


                using (var zip = ZipFile.OpenRead(downloadPath))
                {
                    foreach (var entry in zip.Entries)
                    {
                        if (!string.IsNullOrWhiteSpace(entry.Name)) // These would be directories, but we just copy every file individually... 
                        {
                            var targetPath = Path.Combine(installPath, entry.FullName);
                            // Make sure the folder exists
                            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                            entry.ExtractToFile(targetPath, true);
                        }
                    }
                }

                Console.WriteLine("Starting LuteBot");
                Process.Start(Path.Combine(installPath, "LuteBot.exe"));
                Console.WriteLine("Done!");
                // Let it close itself.  
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine($"\nInstall could not be completed.  You should update manually from {downloadUrl}");
                Console.Read();
            }
        }
    }
}
