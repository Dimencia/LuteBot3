using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using LuteBot.Utils;
using Lutebot.UI;
using LuteBot.playlist;
using System.Threading;
using System.Text.RegularExpressions;
using CsvHelper;

namespace LuteBot.UI
{
    public partial class GuildLibraryForm : Form
    {
        private readonly string appdata_PATH = LuteBotForm.libraryPath;
        private readonly string sheet_ID = "1FLyJ7wpFCCwx6gZ03-CX_S7I9ztqISI9_4mvwu7W2oQ";
        private readonly string log_FILENAME = "logs.txt";
        private readonly string songs_FOLDER = @"songs\";

        private SortableBindingList<Song> SongsList = new SortableBindingList<Song>();

        private LuteBotForm mainForm;

        public GuildLibraryForm(LuteBotForm form, SortableBindingList<Song> inputSongList = null)
        {
            InitializeComponent();
            if (inputSongList != null)
                SongsList = inputSongList;
            InitGridView();
            mainForm = form;
            searchBox.TextChanged += SearchBox_TextChanged;

            Directory.CreateDirectory(appdata_PATH + songs_FOLDER);

            if (File.Exists(appdata_PATH + log_FILENAME))
            {
                //File.Copy(appdata_PATH + log_FILENAME, appdata_PATH + log_FILENAME + DateTime.Now.ToString("MM.dd-HH.mm.ss") + ".backup");
                // No need to keep backups of the log files really
                File.Delete(appdata_PATH + log_FILENAME);
            }
            if (inputSongList == null)
                DownloadGuildLibrary();
        }

        private void InitGridView()
        {
            songGrid.DataSource = typeof(List<Song>);
            songGrid.DataSource = SongsList;

            songGrid.Columns[0].Visible = false; // Hide filename column
            songGrid.Columns[7].Visible = false; // Hide DiscordUrl column
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            UpdateFilteredList();
        }

        private void UpdateFilteredList()
        {
            int previousIndex = songGrid.FirstDisplayedScrollingRowIndex;
            var previousSelected = songGrid.SelectedRows;
            List<Song> selectedSongs = new List<Song>();

            foreach (DataGridViewRow row in previousSelected)
            {
                selectedSongs.Add((Song)row.DataBoundItem);
            }

            string searchString = searchBox.Text.ToLower();
            var filteredBindingList = new SortableBindingList<Song>(SongsList.Where(x =>
            x.Title.ToLower().Contains(searchString) ||
            x.Artist.ToLower().Contains(searchString) ||
            x.Tags.ToLower().Contains(searchString) ||
            x.MidiContributor.ToLower().Contains(searchString)
            ).ToList());

            if (filteredBindingList.Count == 0)
            {
                Song infoSong = new Song
                {
                    Title = $"No songs found, select and add this to try searching Google for {searchString}",
                    Artist = "Make sure to include a full song name and artist for best results, and don't expect good quality",
                    MidiContributor = "",
                    Filename = searchString,
                    Upvotes = "",
                    Tags = "",
                    Compatibility = "",
                    DiscordUrl = ""
                };
                filteredBindingList.Add(infoSong);
            }

            songGrid.DataSource = filteredBindingList;
            if (previousIndex > -1 && songGrid.RowCount > previousIndex)
                songGrid.FirstDisplayedScrollingRowIndex = previousIndex;
            // Seems to always select the first row, let's remove it and re-add if necessary
            if (songGrid.RowCount > 0)
                songGrid.Rows[0].Selected = false;
            foreach (Song s in selectedSongs)
            {
                if (filteredBindingList.Contains(s))
                {
                    foreach (DataGridViewRow row in songGrid.Rows)
                    {
                        if ((Song)row.DataBoundItem == s)
                            row.Selected = true;
                    }
                }
            }

        }

        private byte[] GuildLibrary;

        private void DownloadGuildLibrary()
        {
            // Downloads the excel sheet, reads it, and fills in our songGrid with the data

            string sheet_download_URL = $@"https://docs.google.com/spreadsheets/d/{sheet_ID}/export?format=tsv";

            // Downloads to memory, no longer saves to file so this is no longer necessary
            /*
            if (File.Exists(sheet_FILENAME))
                File.Delete(sheet_FILENAME);
            */

            Log("Beginning download of Master Guild Library list");
            using (WebClient wc = new WebClient())
            {
                wc.DownloadDataCompleted += Wc_DownloadDataCompleted;
                wc.DownloadDataAsync(new Uri(sheet_download_URL));

                //wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
                //wc.DownloadFileAsync(new Uri(sheet_download_URL), appdata_PATH + sheet_FILENAME);
            }
        }

        private void Wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            GuildLibrary = e.Result;
            Log("Download completed, reading library");
            Task.Run(ReadGuildLibrary);
        }

        /*
        private void Wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Log("Download completed, reading library");
            Task.Run(ReadGuildLibrary);
        }
        */

        private static object locker = new object();
        private delegate void SafeCallDelegate(string text);
        private void Log(string message)
        {
            // Make sure our window still exists
            if (this.IsDisposed)
            {
                return;
            }
            Console.WriteLine(message);
            if (InvokeRequired)
            {
                var d = new SafeCallDelegate(Log);
                try
                {
                    Invoke(d, new object[] { message });
                }
                catch (ObjectDisposedException e) { } // If we're disposed just give up
                // Invoke takes a while to run so it becomes likely that this will still be trying to run when we get disposed
            }
            else
            {
                statusLabel.Text = message;
                try
                {
                    lock (locker)
                        using (StreamWriter writer =
                            File.AppendText(appdata_PATH + log_FILENAME))
                        {
                            writer.WriteLine(message);
                        }
                }
                catch (Exception e)
                {
                    Log(e.Message);
                    Log(e.StackTrace);
                    Log($"Failed to write log: {message}");
                }
            }
        }

        private void ReadGuildLibrary()
        {
            // Reads our appdata_PATH+sheet_FILENAME tsv file and saves the data into a list of Songs
            SortableBindingList<Song> newSongs = new SortableBindingList<Song>();

            if (InvokeRequired)
                Invoke((MethodInvoker)delegate
                {
                    SongsList.Clear();
                });

            //using (StreamReader reader = new StreamReader(appdata_PATH + sheet_FILENAME))
            using (TextReader reader = new StreamReader(new MemoryStream(GuildLibrary)))
            {
                var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);
                csv.Configuration.Delimiter = "\t";
                var records = csv.GetRecords<Song>();
                newSongs = new SortableBindingList<Song>(records);

                /*
                // Skip first line, column headers
                reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    // Make sure we're not disposed and get out of here if we are
                    if (this.IsDisposed)
                    {
                        break;
                    }
                    string line = reader.ReadLine();
                    string[] values = line.Split('\t');
                    Song currentSong = new Song();
                    try
                    {
                        currentSong.Title = values[Columns.Title];
                        if (string.IsNullOrEmpty(currentSong.Title))
                            currentSong.Title = values[Columns.Filename];
                        currentSong.Artist = values[Columns.Artist];
                        currentSong.Tags = values[Columns.Tags];
                        currentSong.Compatibility = values[Columns.Compatibility];
                        currentSong.Upvotes = values[Columns.Upvotes];
                        currentSong.MidiContributor = values[Columns.Contributor];
                        currentSong.DiscordUrl = values[Columns.URL];
                        currentSong.Filename = values[Columns.Filename];

                        newSongs.Add(currentSong);
                    }
                    catch (Exception e)
                    {
                        Log(e.StackTrace);
                        Log(e.Message);
                        Log("Failed while parsing song, attempting to continue...");
                        continue;
                    }
                }
                */
            }
            GuildLibrary = null;
            if (!this.IsDisposed)
            {
                Log($"Successfully parsed {newSongs.Count} songs");

                SongsList = newSongs;
                Invoke((MethodInvoker)delegate
                {
                    InitGridView();
                });

                // If we fully parsed all the songs, we should send it back to the main form to hold on to
                // In case this form is closed and reopened later
                AllowColumnSorting();
            }
        }

        private void AllowColumnSorting()
        {
            // Sets up sorting for each column
            foreach (DataGridViewColumn c in songGrid.Columns)
            {
                c.SortMode = DataGridViewColumnSortMode.Automatic;
            }
        }

        static class Columns
        {
            public static int Filename = 1;
            public static int Title = 2;
            public static int Artist = 3;
            public static int Tags = 4;
            public static int Contributor = 5;
            public static int Compatibility = 6;
            public static int Upvotes = 7;
            public static int URL = 8;
            public static int FileSize = 9;
            public static int Checksum = 10;
        }

        private void downloadAndAddSong(Song song)
        {
            // Downloads midi from the URL, returning the path to the song
            string path = appdata_PATH + songs_FOLDER + song.Filename;
            if (File.Exists(path)) // No need to redownload if we have it
            {
                AddSongToPlaylist(song, path);
                return;
            }

            if (song.DiscordUrl.Equals(string.Empty))
            {
                // Indication that we should search google
                song = GetSongDataFromGoogle(song);
                path = appdata_PATH + songs_FOLDER + song.Filename;
            }

            // If it's still empty, we need to abort
            if (song.DiscordUrl.Equals(string.Empty))
            {
                Log($"Unable to find {song.Filename} online");
                return;
            }

            // We need to pass the song when download is completed
            // So we'll do a Task.Run that handles that... hopefully
            Task.Run(() =>
        {
            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.DownloadFile(song.DiscordUrl, path);
                    AddSongToPlaylist(song, path);
                }
                catch (Exception e)
                {
                    Log(e.StackTrace);
                    Log(e.Message);
                    Log($"Failed to download/add {song.Title}");
                }
            }
        });
        }

        private Song GetSongDataFromGoogle(Song song)
        {
            // Misleading name, for now we're just using bitmidi
            // https://bitmidi.com/search?q=test as a search page
            // Results are <a class="pointer no-underline fw4 white underline-hover">, which contain a link
            // Inside that link, a class pointer no-underline fw4 dark-blue underline-hover contains our href to download

            // If we find a download link, we need to set the song's FileName and URL then return it
            // Otherwise right now, the FileName contains the search query
            System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            // For now I'll be lazy and we'll just parse the page as a string
            string searchURL = $@"https://bitmidi.com/search?q={song.Filename}";
            using (WebClient wc = new WebClient())
            {
                string webData = wc.DownloadString(searchURL);
                // Actually there's some real easy javascript in there that lets us get it directly
                // "data":{"midis":{ is how it starts, let's cut to after that
                // Then the first instace of this we find, we take
                // "downloadUrl":"/uploads/19022.mid"
                // Hell... we can just take that part
                Regex reg = new Regex("downloadUrl\":\"([^\"]*)");
                var match = reg.Match(webData);
                if (!match.Success)
                    return song;
                // So... we have our URL in Groups[1]
                string url = $@"https://bitmidi.com{match.Groups[1].Value}";
                // This doesn't give us a name, so we can use this...
                reg = new Regex(",\"name\":\"([^\"]*)");
                match = reg.Match(webData);
                if (!match.Success)
                    return song;
                string filename = match.Groups[1].Value;
                song.Title = filename.Replace(".mid", "");
                song.Filename = filename;
                song.DiscordUrl = url;
                song.Artist = "";
                song.MidiContributor = "Auto-Generated";
                Log($"Successfully found {filename} online, downloading...");
            }
            return song;
        }

        private void AddSongToPlaylist(Song song, string path)
        {
            PlayListItem plsong = new PlayListItem { Name = song.Title, Path = path };
            // If the playlist form isn't open, we can add it to the playlist manager and it will show up when opened
            if (mainForm.playListForm == null || mainForm.playListForm.IsDisposed)
            {
                LuteBotForm.playList.AddTrack(plsong);
            }
            else // If it is open, we need to add it directly do it, so just pass it to the form
            {
                mainForm.playListForm.Invoke((MethodInvoker)delegate
                {
                    mainForm.playListForm.AddSongToPlaylist(plsong);
                });                
            }
            Log($"Successfully added {song.Title} to current playlist");
        }

        private void ButtonAddToPlaylist_Click(object sender, EventArgs e)
        {
            // Iterate over all selected rows and get the Song from each one
            // Then ... add to current playlist?  
            foreach (DataGridViewRow row in songGrid.SelectedRows)
            {
                downloadAndAddSong((Song)row.DataBoundItem);
            }
        }
    }
}
