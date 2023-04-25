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
using Newtonsoft.Json;
using System.Net.Http;

namespace LuteBot.UI
{
    public partial class GuildLibraryForm : Form
    {
        private readonly string appdata_PATH = LuteBotForm.libraryPath;
        private readonly string sheet_ID = "1FLyJ7wpFCCwx6gZ03-CX_S7I9ztqISI9_4mvwu7W2oQ";
        private readonly string log_FILENAME = "logs.txt";
        private readonly string songs_FOLDER = "songs";

        private SortableBindingList<GuildSong> SongsList = new SortableBindingList<GuildSong>();

        private LuteBotForm mainForm;

        public GuildLibraryForm(LuteBotForm form)
        {
            InitializeComponent();
            InitGridView();
            mainForm = form;
            searchBox.TextChanged += SearchBox_TextChanged;
            searchBox.KeyPress += SearchBox_KeyPress;

            Directory.CreateDirectory(Path.Combine(appdata_PATH, songs_FOLDER));

            if (File.Exists(Path.Combine(appdata_PATH, log_FILENAME)))
            {
                //File.Copy(appdata_PATH + log_FILENAME, appdata_PATH + log_FILENAME + DateTime.Now.ToString("MM.dd-HH.mm.ss") + ".backup");
                // No need to keep backups of the log files really
                File.Delete(Path.Combine(appdata_PATH, log_FILENAME));
            }
        }

        private async void SearchBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                searchBox.Enabled = false;
                await UpdateFilteredList().ConfigureAwait(true);
                searchBox.Enabled = true;
            }
        }

        private async void buttonSearch_Click(object sender, EventArgs e)
        {
            searchBox.Enabled = false;
            await UpdateFilteredList().ConfigureAwait(true);
            searchBox.Enabled = true;
        }

        private void InitGridView()
        {
            songGrid.DataSource = typeof(List<GuildSong>);
            songGrid.DataSource = SongsList;
            songGrid.Columns[0].Visible = false; // Hide checksum column
            songGrid.Columns[4].Visible = false; // Hide source_url column

            songGrid.Columns[1].Name = "Filename";
            songGrid.Columns[2].Name = "Contributor";
            songGrid.Columns[3].Name = "Mordhau Score"; // Attribs didn't work... 

            songGrid.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            songGrid.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            songGrid.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells; // Songname fills
            //songGrid.Columns[0].Visible = false; // Hide filename column
            //songGrid.Columns[7].Visible = false; // Hide DiscordUrl column
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            //UpdateFilteredList();
        }

        private async Task UpdateFilteredList()
        {
            int previousIndex = songGrid.FirstDisplayedScrollingRowIndex;
            var previousSelected = songGrid.SelectedRows;
            List<GuildSong> selectedSongs = new List<GuildSong>();

            foreach (DataGridViewRow row in previousSelected)
            {
                selectedSongs.Add((GuildSong)row.DataBoundItem);
            }

            string searchString = "";
            if (searchBox.Text != null && !string.IsNullOrWhiteSpace(searchBox.Text))
                searchString = searchBox.Text.ToLower();
            else
                return; // Give up, he won't respond to empty queries
            SortableBindingList<GuildSong> filteredBindingList;

            // Populate the list with a Guild Library query
            searchString = WebUtility.UrlEncode(searchString);
            string url = $"http://api.bardsguild.life/?key=0Tk-seyqLFwn5qCH2YzrYA&find={searchString}";
            try
            {
                using (WebClient client = new WebClient())
                {
                    string results = await client.DownloadStringTaskAsync(url).ConfigureAwait(true);
                    var songArray = JsonConvert.DeserializeObject<GuildSong[]>(results);
                    filteredBindingList = new SortableBindingList<GuildSong>(songArray);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log("Failed to query API");
                return;
            }


            if (filteredBindingList.Count == 0)
            {
                GuildSong infoSong = new GuildSong
                {
                    filename = $"No songs found, select and add this to try searching Google for {searchString}",
                    contributor = "Make sure to include a full song name and artist for best results, and don't expect good quality"
                };
                filteredBindingList.Add(infoSong);
            }

            songGrid.DataSource = filteredBindingList;
            if (previousIndex > -1 && songGrid.RowCount > previousIndex)
                songGrid.FirstDisplayedScrollingRowIndex = previousIndex;
            // Seems to always select the first row, let's remove it and re-add if necessary
            if (songGrid.RowCount > 0)
                songGrid.Rows[0].Selected = false;
            foreach (GuildSong s in selectedSongs)
            {
                if (filteredBindingList.Contains(s))
                {
                    foreach (DataGridViewRow row in songGrid.Rows)
                    {
                        if ((GuildSong)row.DataBoundItem == s)
                            row.Selected = true;
                    }
                }
            }

        }


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
                            File.AppendText(Path.Combine(appdata_PATH, log_FILENAME)))
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


        private void AllowColumnSorting()
        {
            // Sets up sorting for each column
            foreach (DataGridViewColumn c in songGrid.Columns)
            {
                c.SortMode = DataGridViewColumnSortMode.Automatic;
            }
        }


        private async Task downloadSongs(IEnumerable<GuildSong> songs)
        {
            List<string> paths = new List<string>();
            foreach (var song in songs)
            {
                // Downloads midi from the URL and plays, or adds to playlist, as appropriate
                string path = Path.Combine(appdata_PATH, songs_FOLDER, song.filename);
                if (File.Exists(path)) // No need to redownload if we have it
                {
                    paths.Add(path);
                    continue;
                }

                if (song.source_url == null || song.source_url.Equals(string.Empty))
                {
                    // Try the CDN
                    song.source_url = $"https://storage.googleapis.com/bgml/mid/{song.checksum}.mid";
                }

                // We need to pass the song when download is completed
                // So we'll do a Task.Run that handles that... hopefully

                using (HttpClient wc = new HttpClient())
                {
                    try
                    {
                        using (var stream = await wc.GetStreamAsync(song.source_url).ConfigureAwait(false))
                        using (var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, true))
                        {
                            await stream.CopyToAsync(fs).ConfigureAwait(false);
                            await fs.FlushAsync().ConfigureAwait(false);
                        }
                        paths.Add(path);
                    }
                    catch (Exception e)
                    {
                        await mainForm.HandleError(e, $"Failed to download/add {song.filename} from {song.source_url}").ConfigureAwait(false);
                        Log($"Failed to download/add {song.filename}");
                    }
                }
            }
            await PlaySongs(paths).ConfigureAwait(false);
        }

        private async Task downloadAndPlaySongs(IEnumerable<GuildSong> songs)
        {
            await downloadSongs(songs).ConfigureAwait(false);
        }


        private async Task PlaySongs(IEnumerable<string> paths)
        {
            await mainForm.partitionsForm.AutoSaveFiles(paths).ConfigureAwait(false);
        }


        private async void buttonPlay_Click(object sender, EventArgs e)
        {
            if (songGrid.SelectedRows.Count > 0)
            {
                var selected = new List<GuildSong>();
                foreach (DataGridViewRow s in songGrid.SelectedRows)
                {
                    selected.Add(s.DataBoundItem as GuildSong);
                }
                await downloadAndPlaySongs(selected);
            }
        }
    }
}
