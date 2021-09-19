using LuteBot.Config;
using LuteBot.playlist;
using LuteBot.UI.Utils;
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

namespace LuteBot
{
    public partial class PlayListForm : Form
    {
        private Point mDownPos;
        private PlayListManager playListManager;

        public PlayListForm(PlayListManager playList)
        {
            InitializeComponent();
            this.playListManager = playList;
            this.playListManager.PlayListUpdatedEvent += new EventHandler<PlayListEventArgs>(PlayList_Updated);
            var lastPlayListPath = ConfigManager.GetProperty(PropertyItem.LastPlaylistLocation);
            if (lastPlayListPath != null && lastPlayListPath.Length > 0)
            {
                playListManager.LoadLastPlayList(lastPlayListPath);
            }
            RefreshPlayListBox();
        }

        private void ContextMenuHelper()
        {
            if (PlayListBox.Items.Count > 0 && PlayListBox.SelectedIndex >= 0)
            {
                ContextMenu playListContextMenu = new ContextMenu();

                MenuItem playItem = playListContextMenu.MenuItems.Add("Play");
                playItem.Click += new EventHandler(PlayMenuItem_Click);
                MenuItem deleteItem = playListContextMenu.MenuItems.Add("Remove");
                deleteItem.Click += new EventHandler(DeleteMenuItem_Click);
                PlayListBox.ContextMenu = playListContextMenu;
            }
            else
            {
                PlayListBox.ContextMenu = null;
            }
        }

        private void PlayMenuItem_Click(object sender, EventArgs e)
        {
            playListManager.Play(PlayListBox.SelectedIndex);
            RefreshPlayListBox();
        }

        private void DeleteMenuItem_Click(object sender, EventArgs e)
        {
            playListManager.Remove(PlayListBox.SelectedIndex);
            RefreshPlayListBox();
        }

        private void RefreshPlayListBox()

        {
            PlayListBox.Items.Clear();
            for (int i = 0; i < playListManager.Count(); i++)
            {
                PlayListBox.Items.Add(playListManager.Get(i));
            }
            Refresh();
        }

        private void PlayList_Updated(object sender, PlayListEventArgs e)
        {
            if (e.EventType.Equals(PlayListEventArgs.UpdatedComponent.TrackChanged))
            {
                playListManager.CurrentTrackIndex = e.Id;
            }
            Refresh();
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openMidiFileDialog = new OpenFileDialog()
            {
                DefaultExt = "mid",
                Filter = "MIDI files|*.mid|All files|*.*",
                Title = "Open MIDI file",
                Multiselect = true
            };
            if (openMidiFileDialog.ShowDialog() == DialogResult.OK)
            {
                var fileNames = openMidiFileDialog.FileNames;
                foreach (var file in fileNames)
                {
                    PlayListItem music = new PlayListItem();
                    music.Name = Path.GetFileName(file).Replace(".mid","");
                    music.Path = file;
                    PlayListBox.Items.Add(music);
                    playListManager.AddTrack(music);
                }
            }
        }

        public void AddSongToPlaylist(PlayListItem song)
        {
            PlayListBox.Items.Add(song);
            playListManager.AddTrack(song);
        }

        private class DragObject
        {
            public ListBox source;
            public object item;
            public DragObject(ListBox box, object data) { source = box; item = data; }
        }

        void List_DragEnter(object sender, DragEventArgs e)
        {
            DragObject obj = e.Data.GetData(typeof(DragObject)) as DragObject;
            if (obj != null) e.Effect = e.AllowedEffect;
        }

        void List_DragDrop(object sender, DragEventArgs e)
        {
            DragObject obj = e.Data.GetData(typeof(DragObject)) as DragObject;
            int index = PlayListBox.IndexFromPoint(new Point(PlayListBox.PointToClient(Cursor.Position).X, PlayListBox.PointToClient(Cursor.Position).Y));
            int oldIndex = obj.source.Items.IndexOf(obj.item);
            if (index != oldIndex)
            {
                PlayListItem tempItem = (PlayListItem)obj.item;
                obj.source.Items.Remove(obj.item);
                playListManager.Remove(tempItem);
                if (index < 0 || index >= PlayListBox.Items.Count)
                {
                    PlayListBox.Items.Add(obj.item);
                    playListManager.AddTrack(tempItem);
                }
                else
                {
                    PlayListBox.Items.Insert(index, obj.item);
                    playListManager.InsertTrack(index, tempItem);
                }
            }
        }

        void List_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            int index = PlayListBox.IndexFromPoint(e.Location);
            if (index < 0) return;
            if (Math.Abs(e.X - mDownPos.X) >= SystemInformation.DragSize.Width ||
                Math.Abs(e.Y - mDownPos.Y) >= SystemInformation.DragSize.Height)
                DoDragDrop(new DragObject(PlayListBox, PlayListBox.Items[index]), DragDropEffects.Move);
        }

        void List_MouseDown(object sender, MouseEventArgs e)
        {
            ContextMenuHelper();
        }

        void List_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index >= 0)
            {
                Brush myBrush = Brushes.Black;
                var box = (ListBox)sender;
                var fore = box.ForeColor;

                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) fore = SystemColors.HighlightText;
                string text = "";
                if (box.Items[e.Index].GetType() == typeof(PlayListItem))
                {
                    if (((PlayListItem)box.Items[e.Index]).IsActive)
                    {
                        fore = Color.DarkOrange;
                    }
                    text = ((PlayListItem)box.Items[e.Index]).Name;
                }
                TextRenderer.DrawText(e.Graphics, text,
                    box.Font, e.Bounds, fore, TextFormatFlags.Left);
                e.DrawFocusRectangle();
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            playListManager.SavePlayList();
            RefreshPlayListBox();
        }

        private void PlayListForm_Closing(object sender, FormClosingEventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.LastPlaylistLocation, playListManager.playlist.Path);
            WindowPositionUtils.UpdateBounds(PropertyItem.PlayListPos, new Point() { X = Left, Y = Top });
            ConfigManager.SaveConfig();
            playListManager.Dispose();
        }

        private void LoadPlayListButton_Click(object sender, EventArgs e)
        {
            playListManager.LoadPlayList();
            RefreshPlayListBox();
        }

        private void PlayListForm_ResizeEnd(object sender, EventArgs e)
        {
            Refresh();
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            // Iterate every song in the playlist, copy them to a new folder
            // Let them zip them up however they want

            // First create a folder if there isn't one... 
            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lutebot", "ExportedPlaylists");
            var nameForm = new TrackNamingForm("Playlist");
            nameForm.ShowDialog(this);
            if (nameForm.DialogResult == DialogResult.OK)
            {
                var playlistName = nameForm.textBoxPartName.Text;
                var currentName = playlistName;
                int count = 0;

                while (Directory.Exists(Path.Combine(basePath, currentName)))
                    currentName = $"{playlistName} ({++count})";

                var dirPath = Path.Combine(basePath, currentName);

                Directory.CreateDirectory(dirPath);



                foreach (var o in PlayListBox.Items)
                {
                    if (o is PlayListItem item)
                    {
                        if (File.Exists(item.Path))
                        {
                            var filename = Path.GetFileName(item.Path);
                            File.Copy(item.Path, Path.Combine(dirPath, filename), true);
                        }
                    }
                }
                System.Diagnostics.Process.Start(dirPath);
            }
        }
    }
}
