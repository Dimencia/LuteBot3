using LuteBot.playlist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.Playlist
{
    public class PlayList
    {
        private List<PlayListItem> musicList;
        private int currentTrackIndex;
        private string path;

        public int CurrentTrackIndex { get => currentTrackIndex; set => currentTrackIndex = value; }
        public string Path { get => path; set => path = value; }
        public List<PlayListItem> MusicList { get => musicList; set => musicList = value; }
    }
}
