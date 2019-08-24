using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.Soundboard
{
    public class SoundBoardEventArgs : EventArgs
    {
        private SoundBoardItem selectedTrack;

        public SoundBoardItem SelectedTrack { get => selectedTrack; set => selectedTrack = value; }
    }
}
