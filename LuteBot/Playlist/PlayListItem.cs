using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.playlist
{
    [DataContract]
    public class PlayListItem
    {
        private string name;
        private string path;
        private bool isActive;
        [DataMember]
        public string Name { get => name; set => name = value; }
        [DataMember]
        public string Path { get => path; set => path = value; }
        [DataMember]
        public bool IsActive { get => isActive; set => isActive = value; }
    }
}
