using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.TrackSelection
{
    public class MidiChannelItem
    {
        private int id;
        private bool active;

        public int Id { get => id; set => id = value; }
        public string Name { get; set; }
        public bool Active { get => active; set => active = value; }

        public MidiChannelItem() { }

        public MidiChannelItem(MidiChannelItem old)
        {
            this.Id = old.Id;
            this.Name = old.Name;
            this.Active = old.Active;
        }
    }
}
