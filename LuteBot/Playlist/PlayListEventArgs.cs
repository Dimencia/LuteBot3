using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.playlist
{
    public class PlayListEventArgs : EventArgs
    {
        public enum UpdatedComponent
        {
            UpdateNavButtons, //tell the handler to update the previous and next buttons
            TrackChanged, //tell the handler that the user pressed the next or previous button
            PlayRequest //Tell the handler that a specific track was requested in the playlist
        }

        private UpdatedComponent eventType;

        private int id; //index of the track the event is about

        public UpdatedComponent EventType { get => eventType; set => eventType = value; }
        public int Id { get => id; set => id = value; }
    }
}
