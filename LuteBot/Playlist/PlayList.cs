using LuteBot.playlist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.Playlist
{
    [DataContract]
    public class PlayList
    {
        private List<PlayListItem> musicList;
        private int currentTrackIndex;
        private string path;
        [DataMember]
        public int CurrentTrackIndex { get => currentTrackIndex; set => currentTrackIndex = value; }
        
        public string Path { get => path; set => path = value; }
        [DataMember]
        public List<PlayListItem> MusicList { get => musicList; set => musicList = value; }
    }
}
