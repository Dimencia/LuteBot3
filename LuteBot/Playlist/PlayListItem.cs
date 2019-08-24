using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.playlist
{
    public class PlayListItem
    {
        private string name;
        private string path;
        private bool isActive;

        public string Name { get => name; set => name = value; }
        public string Path { get => path; set => path = value; }
        public bool IsActive { get => isActive; set => isActive = value; }
    }
}
