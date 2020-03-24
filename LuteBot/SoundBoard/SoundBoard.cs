using LuteBot.IO.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.Soundboard
{
    [DataContract]
    public class SoundBoard
    {
        private SoundBoardItem[] soundBoardItems;
        private string location;
        [DataMember]
        public SoundBoardItem[] SoundBoardItems { get => soundBoardItems; set => soundBoardItems = value; }
        
        public string Location { get => location; set => location = value; }
    }
}
