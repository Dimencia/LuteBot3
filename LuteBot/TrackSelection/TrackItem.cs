using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.TrackSelection
{
// TODO: I'd like to just remove this entirely, but that will be real annoying; I can't just rename, and have to hunt down >36 spots to fix
    public class TrackItem : MidiChannelItem
    {

        public TrackItem() { }

        public TrackItem(TrackItem old) : base(old)
        {
        }

        public TrackItem(SimpleMidiChannelItem old) : base(old) { }

        public TrackItem(MidiChannelItem old) : base(old) { }
    }
}
