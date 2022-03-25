using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.TrackSelection
{
    [DataContract]
    public class TrackItem
    {
        private string name;
        private bool active;
        private int id;
        [DataMember]
        public string Name { get => name; set => name = value; }
        [DataMember]
        public bool Active { get => active; set => active = value; }
        [DataMember]
        public int Id { get => id; set => id = value; }
        public int numNotes { get; set; }
        public int highestNote { get; set; }
        public int lowestNote { get; set; }
        public int avgNoteLength { get; set; }

        public TrackItem() { }

        public TrackItem(TrackItem old)
        {
            this.Name = old.Name;
            this.Active = old.Active;
            this.Id = old.Id;
            this.numNotes = old.numNotes;
            this.highestNote = old.highestNote;
            this.lowestNote = old.lowestNote;
            this.avgNoteLength = old.avgNoteLength;
        }
    }
}
