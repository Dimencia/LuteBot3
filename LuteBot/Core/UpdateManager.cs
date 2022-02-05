using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LuteBot.Core
{
    public static class UpdateManager
    {
        // I'm going to pre-compile these because it's a big page.  I might should import some library to handle it more properly, but this works for now
        private static Regex versionRegex = new Regex(@"https:\/\/github\.com\/Dimencia\/LuteBot3\/releases\/tag\/v([0-9]\.[0-9]\.?[0-9]?)", RegexOptions.Compiled);

        private static Regex titleRegex = new Regex(@"<[^>]*class=[""']d-inline mr-3[""'][^>]*>([^<]*)<", RegexOptions.Compiled); // I don't like how specific this one is, github could change this easily
        // I could get it from various other places that are less likely to change, like the <title> tag, but it also contains some extra stuff in it
        // Such as: Release Partition Loading, Startup Crash Fix Â· Dimencia/LuteBot3 Â· GitHub
        // Where the text I want is 'Partition Loading, Startup Crash Fix'.  And again, those look like they could change, so might not be a good idea to just try to replace them out

        private static Regex descriptionRegex = new Regex(@"<[^>]*data-test-selector=[""']body-content[""'][^>]*>([\w\W]*?)<\/div>", RegexOptions.Compiled);
        // This could break if there is a nested div, but there shouldn't be.  We will need to parse out html inside, replace </p> and <br> with \n, remove all others
        // This is where a proper parsing library would really help... but shouldn't be hard to do

        // This one seems pretty safe though.  Note that it outputs like "/Dimencia/LuteBot3/archive/refs/tags/v3.3.2.zip"
        // Well.  I need to make it more specific because there are two, and I'd rather it point at the releases one, not the tag one
        private static Regex downloadRegex = new Regex(@"<a href=[""'](\/Dimencia\/LuteBot3\/releases\/download\/[^""']*\.zip)[""']", RegexOptions.Compiled);

        public static int[] ConvertVersion(string version)
        {
            return version.Split('.').Select(s => int.Parse(s)).ToArray();
        }

        public static async Task<LuteBotVersion> GetLatestVersion()
        {
            var result = new LuteBotVersion();
            var versionPage = await GetVersionPage(10000); // Arbitrary 10s timeout seems fine
            if (versionPage != null)
            {
                var versionMatch = versionRegex.Match(versionPage);
                if (versionMatch.Success)
                {
                    result.Version = versionMatch.Groups[1].Value;
                    result.VersionArray = ConvertVersion(result.Version);

                    var titleMatch = titleRegex.Match(versionPage);
                    if (titleMatch.Success)
                    {
                        result.Title = titleMatch.Groups[1].Value;
                    }

                    var descriptionMatch = descriptionRegex.Match(versionPage);
                    if (descriptionMatch.Success)
                    {
                        // Put newlines on </p> and <br/>, and remove all other HTML tags
                        result.Description = Regex.Replace(Regex.Replace(descriptionMatch.Groups[1].Value, @"(<\/p>)|(<br\/? ?>)", "\n"), "<[^>]*?>", "");
                    }

                    var downloadMatch = downloadRegex.Match(versionPage);
                    if (downloadMatch.Success) // They give a relative path starting at /Dimencia
                        result.DownloadLink = "https://github.com" + downloadMatch.Groups[1].Value;

                    return result;
                }
                return null;
            }
            else
                return null;
        }

        private static async Task<string> GetVersionPage(int timeout)
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12; // Didn't I already do this?
                using (System.Net.WebClient client = new System.Net.WebClient())
                {
                    //var data = client.DownloadData(url);
                    //string downloadString = UTF8Encoding.UTF8.GetString(data);
                    var downloadTask = client.DownloadStringTaskAsync("https://github.com/Dimencia/LuteBot3/releases/latest");
                    var timeoutTask = Task.Delay(timeout);
                    if (await Task.WhenAny(downloadTask, timeoutTask) == downloadTask)
                    {
                        return downloadTask.Result;
                    }
                    else
                    {
                        new UI.PopupForm("Version Check Failed", $"Could not determine the latest LuteBot version",
                    $"Connection to Github timed out\nYou may want to manually check for an updated version at the following link", new Dictionary<string, string>() { { "LuteBot Releases", "https://github.com/Dimencia/LuteBot3/releases" }, { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" } })
                    .ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                new UI.PopupForm("Version Check Failed", $"Could not determine the latest LuteBot version",
                    $"You are offline, or something is wrong\nYou may want to manually check for an updated version at the following link\n\n{ex.Message}\n{ex.StackTrace}", new Dictionary<string, string>() { { "LuteBot Releases", "https://github.com/Dimencia/LuteBot3/releases" }, { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" } })
                    .ShowDialog();
            }
            return null;
        }
    }


    public class LuteBotVersion
    {
        public string Title { get; set; }
        public int[] VersionArray { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string DownloadLink { get; set; }
    }
}
