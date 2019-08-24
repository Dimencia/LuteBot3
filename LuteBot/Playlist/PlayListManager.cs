using LuteBot.IO.Files;
using LuteBot.Playlist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.playlist
{
    public class PlayListManager
    {
        public PlayList playlist;
        public int CurrentTrackIndex { get => playlist.CurrentTrackIndex; set => playlist.CurrentTrackIndex = value; }

        public PlayListManager()
        {
            playlist = new PlayList();
            playlist.MusicList = new List<PlayListItem>();
            playlist.CurrentTrackIndex = 0;
        }

        public event EventHandler<PlayListEventArgs> PlayListUpdatedEvent;

        public void SavePlayList()
        {
            SaveManager.SavePlayList(playlist);
        }

        public void LoadPlayList()
        {
            PlayList tempPlaylist = SaveManager.LoadPlayList();
            if (tempPlaylist != null)
            {
                this.playlist = tempPlaylist;
                EventHelper(PlayListEventArgs.UpdatedComponent.TrackChanged, 0);
                EventHelper(PlayListEventArgs.UpdatedComponent.UpdateNavButtons, -1);
            }
        }

        public void LoadLastPlayList(string path)
        {
            PlayList tempPlaylist = SaveManager.LoadLastPlayList(path);
            if (tempPlaylist != null)
            {
                this.playlist = tempPlaylist;
                EventHelper(PlayListEventArgs.UpdatedComponent.TrackChanged, 0);
                EventHelper(PlayListEventArgs.UpdatedComponent.UpdateNavButtons, -1);
            }
        }

        public int Count()
        {
            return playlist.MusicList.Count;
        }

        public void Dispose()
        {
            playlist = new PlayList();
            playlist.MusicList = new List<PlayListItem>();
            playlist.CurrentTrackIndex = 0;
            EventHelper(PlayListEventArgs.UpdatedComponent.UpdateNavButtons, -1);
        }

        public void Next()
        {
            playlist.MusicList[playlist.CurrentTrackIndex].IsActive = false;
            if (playlist.CurrentTrackIndex + 1 < playlist.MusicList.Count)
            {
                playlist.CurrentTrackIndex++;
            }
            else
            {
                playlist.CurrentTrackIndex = 0;
            }
            playlist.MusicList[playlist.CurrentTrackIndex].IsActive = true;
            EventHelper(PlayListEventArgs.UpdatedComponent.TrackChanged, playlist.CurrentTrackIndex);
        }

        public void Play(int trackId)
        {
            playlist.MusicList[playlist.CurrentTrackIndex].IsActive = false;
            playlist.CurrentTrackIndex = trackId;
            playlist.MusicList[playlist.CurrentTrackIndex].IsActive = true;
            EventHelper(PlayListEventArgs.UpdatedComponent.PlayRequest, playlist.CurrentTrackIndex);
        }

        public void Previous()
        {
            playlist.MusicList[playlist.CurrentTrackIndex].IsActive = false;
            if (playlist.CurrentTrackIndex - 1 < 0)
            {
                playlist.CurrentTrackIndex = playlist.MusicList.Count - 1;
            }
            else
            {
                playlist.CurrentTrackIndex--;
            }
            playlist.MusicList[playlist.CurrentTrackIndex].IsActive = true;
            EventHelper(PlayListEventArgs.UpdatedComponent.TrackChanged, playlist.CurrentTrackIndex);
        }

        private void EventHelper(PlayListEventArgs.UpdatedComponent component, int trackId)
        {
            EventHandler<PlayListEventArgs> handler = PlayListUpdatedEvent;
            PlayListEventArgs eventArgs = new PlayListEventArgs();
            eventArgs.EventType = component;
            eventArgs.Id = trackId;
            handler?.Invoke(this, eventArgs);
        }

        public PlayListItem Get(int index)
        {
            return playlist.MusicList[index];
        }

        public void AddTrack(PlayListItem item)
        {
            playlist.MusicList.Add(item);
            if (playlist.MusicList.Count == 2)
            {
                EventHelper(PlayListEventArgs.UpdatedComponent.UpdateNavButtons, -1);
            }
        }

        public void InsertTrack(int index, PlayListItem item)
        {
            playlist.MusicList.Insert(index, item);
            if (playlist.MusicList.Count == 2)
            {
                EventHelper(PlayListEventArgs.UpdatedComponent.UpdateNavButtons, -1);
            }
        }

        public void Remove(int index)
        {
            playlist.MusicList.RemoveAt(index);
            if (playlist.MusicList.Count == 1)
            {
                EventHelper(PlayListEventArgs.UpdatedComponent.UpdateNavButtons, -1);
            }
        }

        public void Remove(PlayListItem item)
        {
            playlist.MusicList.Remove(item);
            if (playlist.MusicList.Count == 1)
            {
                EventHelper(PlayListEventArgs.UpdatedComponent.UpdateNavButtons, -1);
            }
        }

        public bool HasNext()
        {
            return playlist.MusicList.Count > 1;
        }
    }
}
